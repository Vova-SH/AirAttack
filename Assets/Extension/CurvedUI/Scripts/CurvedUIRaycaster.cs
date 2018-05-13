﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;


namespace CurvedUI
{

	public class CurvedUIRaycaster : GraphicRaycaster
    {

        [SerializeField]
        bool showDebug = false;



        //Settings--------------------------------------//

        // CurvedUIRaycaster must modify the position of the eventData to make it valid for the curved canvas. 
        // It can either create a copy, or override the original. The copy will only be used for this canvas, in this frame. 
        // The overridden original will be carried to other canvases and next frames.
        //
        // Set this to TRUE if this raycaster should override the original eventData.
        // Overriding eventData allows canvas to use 1:1 scrolling. Scroll rects and sliders behave as they should on a curved surface and follow the pointer.
        // This however breaks the interactions with flat canvases in the same scene as original eventData will not be correct for them any more. 
        //
        // Setting this to FALSE will create a copy of the eventData for each canvas.
        // Flat canvases on the same scene will work fine, but scroll rects on curved canvases will move faster / slower than the pointer.
        // May break dragging and scrolling as there will be no past eventdata to calculate delta position from.
        //
        // default true.
        bool overrideEventData = true;




        //Variables --------------------------------------//
        Canvas myCanvas;
        CurvedUISettings mySettings;
        Vector3 cyllinderMidPoint;
        List<GameObject> objectsUnderPointer = new List<GameObject>();
        Vector2 lastCanvasPos = Vector2.zero;
        GameObject colliderContainer;
        PointerEventData lastFrameEventData;

        //gaze click
        List<GameObject> selectablesUnderGaze = new List<GameObject>();
        List<GameObject> selectablesUnderGazeLastFrame = new List<GameObject>();
        float objectsUnderGazeLastChangeTime;
        bool gazeClickExecuted = false;
        bool pointingAtCanvas = false;




        #region LIFECYCLE
        protected override void Awake()
        {
            base.Awake();
            myCanvas = GetComponent<Canvas>();
            mySettings = GetComponent<CurvedUISettings>();

            cyllinderMidPoint = new Vector3(0, 0, -mySettings.GetCyllinderRadiusInCanvasSpace());

            //the canvas needs an event camera set up to process events correctly. Try to use main camera if no one is provided.
            if (myCanvas.worldCamera == null && Camera.main != null)
                myCanvas.worldCamera = Camera.main;
        }

        protected override void Start()
        {
            CreateCollider();
        }

        protected virtual void Update()
        {
            // a way to reset GazeClick when user look away from the canvas
            // not needed in most projects, but left here for reference.
            //if (!pointingAtCanvas)
            //{
            //    ResetGazeTimedClick();
            //    return;
            //}

            //Gaze click process.
            if (CurvedUIInputModule.ControlMethod == CurvedUIInputModule.CUIControlMethod.GAZE && CurvedUIInputModule.Instance.GazeUseTimedClick)
            {
                if (pointingAtCanvas)
                {
                    ProcessGazeTimedClick();

                    //save current selectablesUnderGaze
                    selectablesUnderGazeLastFrame.Clear();
                    selectablesUnderGazeLastFrame.AddRange(selectablesUnderGaze);

                    //find selectables we're currently pointing at in objects under pointer
                    selectablesUnderGaze.Clear();
                    selectablesUnderGaze.AddRange(objectsUnderPointer);
                    selectablesUnderGaze.RemoveAll(delegate (GameObject obj)
                    {
                        return obj.GetComponent<Selectable>() == null;
                    });
                }
                else //not poiting at canvas, reset the timer.
                    ResetGazeTimedClick();


                //Animate progress bar
                if (CurvedUIInputModule.Instance.GazeTimedClickProgressImage != null)
                {
                    if (CurvedUIInputModule.Instance.GazeTimedClickProgressImage.type != Image.Type.Filled)
                        CurvedUIInputModule.Instance.GazeTimedClickProgressImage.type = Image.Type.Filled;

                    CurvedUIInputModule.Instance.GazeTimedClickProgressImage.fillAmount =
                        (Time.time - objectsUnderGazeLastChangeTime).RemapAndClamp(CurvedUIInputModule.Instance.GazeClickTimerDelay, CurvedUIInputModule.Instance.GazeClickTimer + CurvedUIInputModule.Instance.GazeClickTimerDelay, 0, 1);
                }

            } 

            //reset this variable. It will be checked again during next Raycast method run.
            pointingAtCanvas = false;
        }
        #endregion


        #region GAZE INTERACTION
        void ProcessGazeTimedClick()
        {
            //debug
            //string str = " Object under pointer: ";
            //foreach (GameObject go in objectsUnderPointer) str += go.name + ", ";
            //Debug.Log(str);

            //two lists are not the same - selected objects changed
            if (selectablesUnderGazeLastFrame.Count == 0 || selectablesUnderGazeLastFrame.Count != selectablesUnderGaze.Count)
            {
                ResetGazeTimedClick();
                return;
            }

            //Check if objects changed since last frame
            for (int i = 0; i < selectablesUnderGazeLastFrame.Count && i < selectablesUnderGaze.Count; i++)
            {
                if (selectablesUnderGazeLastFrame[i].GetInstanceID() != selectablesUnderGaze[i].GetInstanceID())
                {
                    ResetGazeTimedClick();
                    return;
                }
            }

            //Check if time is done and we havent executed the click yet
            if (!gazeClickExecuted && Time.time > objectsUnderGazeLastChangeTime + CurvedUIInputModule.Instance.GazeClickTimer + CurvedUIInputModule.Instance.GazeClickTimerDelay)
            {
                Click();
                gazeClickExecuted = true;
            }
        }

        void ResetGazeTimedClick()
        {
            objectsUnderGazeLastChangeTime = Time.time;
            gazeClickExecuted = false;
        }
        #endregion


        #region RAYCASTING
        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {

            if (!mySettings.Interactable)
                return;

            //check if we have a world camera to process events by
            if (myCanvas.worldCamera == null)
                Debug.LogWarning("CurvedUIRaycaster requires Canvas to have a world camera reference to process events!", myCanvas.gameObject);

            Camera worldCamera = myCanvas.worldCamera;
            Ray ray3D;

            //get a ray to raycast with depending on the control method
            switch (CurvedUIInputModule.ControlMethod)
            {
                case CurvedUIInputModule.CUIControlMethod.MOUSE:
                {
                    // Get a ray from the camera through the point on the screen - used for mouse input
                    ray3D = worldCamera.ScreenPointToRay(eventData.position);
                    break;
                }
                case CurvedUIInputModule.CUIControlMethod.GAZE:
                {
                    //get a ray from the center of world camera. used for gaze input
                    ray3D = new Ray(worldCamera.transform.position, worldCamera.transform.forward);

                    UpdateSelectedObjects(eventData);

                    break;
                }
                case CurvedUIInputModule.CUIControlMethod.WORLD_MOUSE:
                {
                    // Get a ray set in CustromControllerRay property
                    ray3D = new Ray(worldCamera.transform.position, (mySettings.CanvasToCurvedCanvas(CurvedUIInputModule.Instance.WorldSpaceMouseInCanvasSpace) - myCanvas.worldCamera.transform.position));
                    break;
                }
                case CurvedUIInputModule.CUIControlMethod.STEAMVR:
                {
#if CURVEDUI_VIVE
					// Get a ray from proper controller.
                    if ((eventData as CurvedUIPointerEventData).Controller == CurvedUIInputModule.Right.gameObject)
                    {
                        ray3D = new Ray(CurvedUIInputModule.Right.PointingOrigin, CurvedUIInputModule.Right.PointingDirection);
                    }
                    else if ((eventData as CurvedUIPointerEventData).Controller == CurvedUIInputModule.Left.gameObject)
                    {
                        ray3D = new Ray(CurvedUIInputModule.Left.PointingOrigin, CurvedUIInputModule.Left.PointingDirection);
                    }
                    else
                    {
                        ray3D = new Ray();
                        Debug.LogWarning("CurvedUI: Unknown controller");
                    }

                    break;
#else
                    goto case CurvedUIInputModule.CUIControlMethod.CUSTOM_RAY;
#endif
                }
                case CurvedUIInputModule.CUIControlMethod.OCULUSVR:
                {
                    goto case CurvedUIInputModule.CUIControlMethod.CUSTOM_RAY;
                }
                case CurvedUIInputModule.CUIControlMethod.CUSTOM_RAY:
                {
                    // Get a ray set in CustromControllerRay property
                    ray3D = CurvedUIInputModule.CustomControllerRay;
                    //ProcessMove(eventData);

                    UpdateSelectedObjects(eventData);
                    break;
                }
//                case CurvedUIInputModule.CUIControlMethod.DAYDREAM: //deprecated
//                {
//                    goto case CurvedUIInputModule.CUIControlMethod.CUSTOM_RAY;
//                }
                case CurvedUIInputModule.CUIControlMethod.GOOGLEVR:
                {
#if CURVEDUI_GOOGLEVR
					ray3D = new Ray(GvrPointerInputModule.Pointer.PointerTransform.position, GvrPointerInputModule.Pointer.PointerTransform.forward);
					break;
#else
					Debug.LogError("CURVEDUI: Missing GoogleVR support code. Enable GoogleVR control method on CurvedUISettings component.");
                    goto case CurvedUIInputModule.CUIControlMethod.GAZE;
#endif 
                }
                default:
                {
                    ray3D = new Ray();
                    break;
                }
            }


            //Create a copy of the eventData to be used by this canvas. This allows
            PointerEventData newEventData = new PointerEventData(EventSystem.current);
            if (!overrideEventData)
            {
                newEventData.pointerEnter = eventData.pointerEnter;
                newEventData.rawPointerPress = eventData.rawPointerPress;
                newEventData.pointerDrag = eventData.pointerDrag;
                newEventData.pointerCurrentRaycast = eventData.pointerCurrentRaycast;
                newEventData.pointerPressRaycast = eventData.pointerPressRaycast;
                newEventData.hovered = new List<GameObject>();
                newEventData.hovered.AddRange(eventData.hovered);
                newEventData.eligibleForClick = eventData.eligibleForClick;
                newEventData.pointerId = eventData.pointerId;
                newEventData.position = eventData.position;
                newEventData.delta = eventData.delta;
                newEventData.pressPosition = eventData.pressPosition;
                newEventData.clickTime = eventData.clickTime;
                newEventData.clickCount = eventData.clickCount;
                newEventData.scrollDelta = eventData.scrollDelta;
                newEventData.useDragThreshold = eventData.useDragThreshold;
                newEventData.dragging = eventData.dragging;
                newEventData.button = eventData.button;
            }



            if (mySettings.Angle != 0 && mySettings.enabled)
            { // use custom raycasting only if Curved effect is enabled



                //Getting remappedPosition on the curved canvas ------------------------------//
                //This will be later passed to GraphicRaycaster so it can discover interactions as usual.
                //If we did not hit the curved canvas, return - no interactions are possible
			

                //Physical raycast to find interaction point
                Vector2 remappedPosition = eventData.position;
                switch (mySettings.Shape)
                {
                    case CurvedUISettings.CurvedUIShape.CYLINDER:
                    {
                        if (!RaycastToCyllinderCanvas(ray3D, out remappedPosition, false)) return;
                        break;
                    }
                    case CurvedUISettings.CurvedUIShape.CYLINDER_VERTICAL:
                    {
                        if (!RaycastToCyllinderVerticalCanvas(ray3D, out remappedPosition, false)) return;
                        break;
                    }
                    case CurvedUISettings.CurvedUIShape.RING:
                    {
                        if (!RaycastToRingCanvas(ray3D, out remappedPosition, false)) return;
                        break;
                    }
                    case CurvedUISettings.CurvedUIShape.SPHERE:
                    {
                        if (!RaycastToSphereCanvas(ray3D, out remappedPosition, false)) return;
                        break;
                    }
                }

                //if we got here, it means user is pointing at this canvas.
                pointingAtCanvas = true;


                //Creating eventData for canvas Raycasting -------------------//
                //Which eventData were going to use?
                PointerEventData eventDataToUse = overrideEventData ? eventData : newEventData;

                // Swap event data pressPosition to our remapped pos if this is the frame of the press
                if (eventDataToUse.pressPosition == eventDataToUse.position)
                    eventDataToUse.pressPosition = remappedPosition;

                // Swap event data position to our remapped pos
                eventDataToUse.position = remappedPosition;





                //Scroll Handling---------------------------------------------//
                //We must handle scroll a little differently on these platforms
                if (CurvedUIInputModule.ControlMethod == CurvedUIInputModule.CUIControlMethod.STEAMVR)
                {
                    eventDataToUse.delta = remappedPosition - lastCanvasPos;
                    lastCanvasPos = remappedPosition;
                }
                //else if (CurvedUIInputModule.ControlMethod == CurvedUIInputModule.CUIControlMethod.GAZE)
                //{
                //    //Test for selected object being dragged and initialize dragging, if needed.
                //    //We do this here to trick unity's StandAloneInputModule into thinking we used a touch or mouse to do it.
                //    if (eventData.IsPointerMoving() && eventData.pointerDrag != null && !eventData.dragging
                //    && ShouldStartDrag(eventData.pressPosition, eventData.position, EventSystem.current.pixelDragThreshold, eventData.useDragThreshold))
                //    {
                //        ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.beginDragHandler);
                //        eventData.dragging = true;
                //    }
                //}
            }



            //store objects under pointer so they can quickly retrieved if needed by other scripts
            objectsUnderPointer = eventData.hovered;

            lastFrameEventData = eventData;

            // Use base class raycast method to finish the raycast if we hit anything
            base.Raycast(overrideEventData ? eventData : newEventData, resultAppendList);

        }



		LayerMask GetLayerMaskForMyLayer(){
			int myLayerMask = -1;
			if (mySettings.RaycastMyLayerOnly)
			{
				myLayerMask = 1 << this.gameObject.layer;
			}
			return myLayerMask;
		}


        public virtual bool RaycastToCyllinderCanvas(Ray ray3D, out Vector2 o_canvasPos, bool OutputInCanvasSpace = false)
        {

            if (showDebug)
            {
                Debug.DrawLine(ray3D.origin, ray3D.GetPoint(1000), Color.red);
            }

			LayerMask myLayerMask = GetLayerMaskForMyLayer();

            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray3D, out hit, float.PositiveInfinity, myLayerMask))
            {
                //find if we hit this canvas - this needs to be uncommented
                if (overrideEventData && hit.collider.gameObject != this.gameObject && (colliderContainer == null || hit.collider.transform.parent != colliderContainer.transform))
                {
                    o_canvasPos = Vector2.zero;
                    return false;
                }

                //direction from the cyllinder center to the hit point
                Vector3 localHitPoint = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(hit.point);
                Vector3 directionFromCyllinderCenter = (localHitPoint - cyllinderMidPoint).normalized;

                //angle between middle of the projected canvas and hit point direction
                float angle = -AngleSigned(directionFromCyllinderCenter.ModifyY(0), mySettings.Angle < 0 ? Vector3.back : Vector3.forward, Vector3.up);

                //convert angle to canvas coordinates
                Vector2 canvasSize = myCanvas.GetComponent<RectTransform>().rect.size;

                //map the intersection point to 2d point in canvas space
                Vector2 pointOnCanvas = new Vector3(0, 0, 0);
                pointOnCanvas.x = angle.Remap(-mySettings.Angle / 2.0f, mySettings.Angle / 2.0f, -canvasSize.x / 2.0f, canvasSize.x / 2.0f);
                pointOnCanvas.y = localHitPoint.y;


                if (OutputInCanvasSpace)
                    o_canvasPos = pointOnCanvas;
                else //convert the result to screen point in camera. This will be later used by raycaster and world camera to determine what we're pointing at
                    o_canvasPos = myCanvas.worldCamera.WorldToScreenPoint(myCanvas.transform.localToWorldMatrix.MultiplyPoint3x4(pointOnCanvas));

                if (showDebug)
                {
                    Debug.DrawLine(hit.point, hit.point.ModifyY(hit.point.y + 10), Color.green);
                    Debug.DrawLine(hit.point, myCanvas.transform.localToWorldMatrix.MultiplyPoint3x4(cyllinderMidPoint), Color.yellow);
                }

                return true;
            }

            o_canvasPos = Vector2.zero;
            return false;
        }

        public virtual bool RaycastToCyllinderVerticalCanvas(Ray ray3D, out Vector2 o_canvasPos, bool OutputInCanvasSpace = false)
        {

            if (showDebug)
            {
                Debug.DrawLine(ray3D.origin, ray3D.GetPoint(1000), Color.red);
            }


			LayerMask myLayerMask = GetLayerMaskForMyLayer();

            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray3D, out hit, float.PositiveInfinity, myLayerMask))
            {
                //find if we hit this canvas
                if (overrideEventData && hit.collider.gameObject != this.gameObject && (colliderContainer == null || hit.collider.transform.parent != colliderContainer.transform))
                {
                    o_canvasPos = Vector2.zero;
                    return false;
                }

                //direction from the cyllinder center to the hit point
                Vector3 localHitPoint = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(hit.point);
                Vector3 directionFromCyllinderCenter = (localHitPoint - cyllinderMidPoint).normalized;

                //angle between middle of the projected canvas and hit point direction
                float angle = -AngleSigned(directionFromCyllinderCenter.ModifyX(0), mySettings.Angle < 0 ? Vector3.back : Vector3.forward, Vector3.left);

                //convert angle to canvas coordinates
                Vector2 canvasSize = myCanvas.GetComponent<RectTransform>().rect.size;

                //map the intersection point to 2d point in canvas space
                Vector2 pointOnCanvas = new Vector3(0, 0, 0);
                pointOnCanvas.y = angle.Remap(-mySettings.Angle / 2.0f, mySettings.Angle / 2.0f, -canvasSize.y / 2.0f, canvasSize.y / 2.0f);
                pointOnCanvas.x = localHitPoint.x;


                if (OutputInCanvasSpace)
                    o_canvasPos = pointOnCanvas;
                else //convert the result to screen point in camera. This will be later used by raycaster and world camera to determine what we're pointing at
                    o_canvasPos = myCanvas.worldCamera.WorldToScreenPoint(myCanvas.transform.localToWorldMatrix.MultiplyPoint3x4(pointOnCanvas));

                if (showDebug)
                {
                    Debug.DrawLine(hit.point, hit.point.ModifyY(hit.point.y + 10), Color.green);
                    Debug.DrawLine(hit.point, myCanvas.transform.localToWorldMatrix.MultiplyPoint3x4(cyllinderMidPoint), Color.yellow);
                }

                return true;
            }

            o_canvasPos = Vector2.zero;
            return false;
        }

        public virtual bool RaycastToRingCanvas(Ray ray3D, out Vector2 o_canvasPos, bool OutputInCanvasSpace = false)
        {
			LayerMask myLayerMask = GetLayerMaskForMyLayer();

            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray3D, out hit, float.PositiveInfinity, myLayerMask))
            {
                //find if we hit this canvas
                if (overrideEventData && hit.collider.gameObject != this.gameObject && (colliderContainer == null || hit.collider.transform.parent != colliderContainer.transform))
                {
                    o_canvasPos = Vector2.zero;
                    return false;
                }


                //local hit point on canvas and a direction from center
                Vector3 localHitPoint = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(hit.point);
                Vector3 directionFromRingCenter = localHitPoint.ModifyZ(0).normalized;
                Vector2 canvasSize = myCanvas.GetComponent<RectTransform>().rect.size;


                //angle between middle of the projected canvas and hit point direction from center
                float angle = -AngleSigned(directionFromRingCenter.ModifyZ(0), Vector3.up, Vector3.back);

                //map the intersection point to 2d point in canvas space
                Vector2 pointOnCanvas = new Vector2(0, 0);

                if (showDebug)
                    Debug.Log("angle: " + angle);


                //map x coordinate based on angle between vector up and direction to hitpoint
                if (angle < 0)
                {
                    pointOnCanvas.x = angle.Remap(0, -mySettings.Angle, -canvasSize.x / 2.0f, canvasSize.x / 2.0f);
                }
                else {
                    pointOnCanvas.x = angle.Remap(360, 360 - mySettings.Angle, -canvasSize.x / 2.0f, canvasSize.x / 2.0f);
                }


                //map y coordinate based on hitpoint distance from the center and external diameter
                pointOnCanvas.y = localHitPoint.magnitude.Remap(mySettings.RingExternalDiameter * 0.5f * (1 - mySettings.RingFill), mySettings.RingExternalDiameter * 0.5f,
                    -canvasSize.y * 0.5f * (mySettings.RingFlipVertical ? -1 : 1), canvasSize.y * 0.5f * (mySettings.RingFlipVertical ? -1 : 1));


                if (OutputInCanvasSpace)
                    o_canvasPos = pointOnCanvas;
                else //convert the result to screen point in camera. This will be later used by raycaster and world camera to determine what we're pointing at
                    o_canvasPos = myCanvas.worldCamera.WorldToScreenPoint(myCanvas.transform.localToWorldMatrix.MultiplyPoint3x4(pointOnCanvas));
                return true;
            }

            o_canvasPos = Vector2.zero;
            return false;
        }


        public virtual bool RaycastToSphereCanvas(Ray ray3D, out Vector2 o_canvasPos, bool OutputInCanvasSpace = false)
        {
			LayerMask myLayerMask = GetLayerMaskForMyLayer();

            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray3D, out hit, float.PositiveInfinity, myLayerMask))
            {
                //find if we hit this canvas
                if (overrideEventData && hit.collider.gameObject != this.gameObject && (colliderContainer == null || hit.collider.transform.parent != colliderContainer.transform))
                {
                    o_canvasPos = Vector2.zero;
                    return false;
                }

                Vector2 canvasSize = myCanvas.GetComponent<RectTransform>().rect.size;
                float radius = (mySettings.PreserveAspect ? mySettings.GetCyllinderRadiusInCanvasSpace() : canvasSize.x / 2.0f);

                //local hit point on canvas, direction from its center and a vector perpendicular to direction, so we can use it to calculate its angle in both planes.
                Vector3 localHitPoint = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(hit.point);
                Vector3 SphereCenter = new Vector3(0, 0, mySettings.PreserveAspect ? -radius : 0);
                Vector3 directionFromSphereCenter = (localHitPoint - SphereCenter).normalized;
                Vector3 XZPlanePerpendicular = Vector3.Cross(directionFromSphereCenter, directionFromSphereCenter.ModifyY(0)).normalized * (directionFromSphereCenter.y < 0 ? 1 : -1);

                //horizontal and vertical angle between middle of the sphere and the hit point.
                //We do some fancy checks to determine vectors we compare them to,
                //to make sure they are negative on the left and bottom side of the canvas
                float hAngle = -AngleSigned(directionFromSphereCenter.ModifyY(0), (mySettings.Angle > 0 ? Vector3.forward : Vector3.back), (mySettings.Angle > 0 ? Vector3.up : Vector3.down));
                float vAngle = -AngleSigned(directionFromSphereCenter, directionFromSphereCenter.ModifyY(0), XZPlanePerpendicular);

                //find the size of the canvas expressed as measure of the arc it occupies on the sphere
                float hAngularSize = Mathf.Abs(mySettings.Angle) * 0.5f;
                float vAngularSize = Mathf.Abs(mySettings.PreserveAspect ? hAngularSize * canvasSize.y / canvasSize.x : mySettings.VerticalAngle * 0.5f);

                //map the intersection point to 2d point in canvas space
                Vector2 pointOnCanvas = new Vector2(hAngle.Remap(-hAngularSize, hAngularSize, -canvasSize.x * 0.5f, canvasSize.x * 0.5f),
                                                    vAngle.Remap(-vAngularSize, vAngularSize, -canvasSize.y * 0.5f, canvasSize.y * 0.5f));

                if (showDebug)
                {
                    Debug.Log("h: " + hAngle + " / v: " + vAngle + " poc: " + pointOnCanvas);
                    Debug.DrawRay(myCanvas.transform.localToWorldMatrix.MultiplyPoint3x4(SphereCenter), myCanvas.transform.localToWorldMatrix.MultiplyVector(directionFromSphereCenter) * Mathf.Abs(radius), Color.red);
                    Debug.DrawRay(myCanvas.transform.localToWorldMatrix.MultiplyPoint3x4(SphereCenter), myCanvas.transform.localToWorldMatrix.MultiplyVector(XZPlanePerpendicular) * 300, Color.magenta);
                }

                if (OutputInCanvasSpace)
                    o_canvasPos = pointOnCanvas;
                else // convert the result to screen point in camera.This will be later used by raycaster and world camera to determine what we're pointing at
                    o_canvasPos = myCanvas.worldCamera.WorldToScreenPoint(myCanvas.transform.localToWorldMatrix.MultiplyPoint3x4(pointOnCanvas));

                return true;
            }

            o_canvasPos = Vector2.zero;
            return false;
        }
        #endregion



        #region COLLIDER MANAGEMENT

        /// <summary>
        /// Creates a mesh collider for curved canvas based on current angle and curve segments.
        /// </summary>
        /// <returns>The collider.</returns>
        protected void CreateCollider()
        {

            //remove all colliders on this object
            List<Collider> Cols = new List<Collider>();
            Cols.AddRange(this.GetComponents<Collider>());
            for (int i = 0; i < Cols.Count; i++)
            {
                Destroy(Cols[i]);
            }

            if (!mySettings.BlocksRaycasts) return; //null;

            if (mySettings.Shape == CurvedUISettings.CurvedUIShape.SPHERE && !mySettings.PreserveAspect && mySettings.VerticalAngle == 0) return;// null;

            //create a collider based on mapping type
            switch (mySettings.Shape)
            {

                case CurvedUISettings.CurvedUIShape.CYLINDER:
                {

                    //creating a convex (lower performance - many parts) collider for when we have a rigidbody attached
                    if (mySettings.ForceUseBoxCollider || GetComponent<Rigidbody>() != null || GetComponentInParent<Rigidbody>() != null)
                    {
                        if (colliderContainer != null)
                            GameObject.Destroy(colliderContainer);

                        colliderContainer = CreateConvexCyllinderCollider();
                    }
                    else // create a faster single mesh collier when possible
                    {
                        SetupMeshColliderUsingMesh(CreateCyllinderColliderMesh());
                    }
                    return;
                }
                case CurvedUISettings.CurvedUIShape.CYLINDER_VERTICAL:
                {
                    //creating a convex (lower performance - many parts) collider for when we have a rigidbody attached
                    if (mySettings.ForceUseBoxCollider || GetComponent<Rigidbody>() != null || GetComponentInParent<Rigidbody>() != null)
                    {
                        if (colliderContainer != null)
                            GameObject.Destroy(colliderContainer);

                        colliderContainer = CreateConvexCyllinderCollider(true);
                    }
                    else // create a faster single mesh collier when possible
                    {
                        SetupMeshColliderUsingMesh(CreateCyllinderColliderMesh(true));
                    }
                    return;
                }

                case CurvedUISettings.CurvedUIShape.RING:
                {
                    BoxCollider col = this.gameObject.AddComponent<BoxCollider>();
                    col.size = new Vector3(mySettings.RingExternalDiameter, mySettings.RingExternalDiameter, 1.0f);
                    return;
                }

                case CurvedUISettings.CurvedUIShape.SPHERE:
                {
                    //rigidbody in parent?
                    if (GetComponent<Rigidbody>() != null || GetComponentInParent<Rigidbody>() != null)
                        Debug.LogWarning("CurvedUI: Sphere shape canvases as children of rigidbodies do not support user input. Switch to Cyllinder shape or remove the rigidbody from parent.", this.gameObject);

                    SetupMeshColliderUsingMesh(CreateSphereColliderMesh());
                    return;
                }
                default:
                {
                    return;
                }
            }

        }

        /// <summary>
        /// Adds neccessary components and fills them with given mesh data.
        /// </summary>
        /// <param name="meshie"></param>
        void SetupMeshColliderUsingMesh(Mesh meshie)
        {
            MeshFilter mf = this.AddComponentIfMissing<MeshFilter>();
            MeshCollider mc = this.gameObject.AddComponent<MeshCollider>();
            mf.mesh = meshie;
            mc.sharedMesh = meshie;
        }


        GameObject CreateConvexCyllinderCollider(bool vertical = false)
        {

            GameObject go = new GameObject("_CurvedUIColliders");
            go.layer = this.gameObject.layer;
            go.transform.SetParent(this.transform);
            go.transform.ResetTransform();

            Mesh meshie = new Mesh();

            Vector3[] Vertices = new Vector3[4];
            (myCanvas.transform as RectTransform).GetWorldCorners(Vertices);
            meshie.vertices = Vertices;

            //rearrange them to be in an easy to interpolate order and convert to canvas local spce
            if (vertical)
            {
                Vertices[0] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[1]);
                Vertices[1] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[2]);
                Vertices[2] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[0]);
                Vertices[3] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[3]);
            }
            else
            {
                Vertices[0] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[1]);
                Vertices[1] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[0]);
                Vertices[2] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[2]);
                Vertices[3] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[3]);
            }

            meshie.vertices = Vertices;

            //create a new array of vertices, subdivided as needed
            List<Vector3> verts = new List<Vector3>();
            int vertsCount = Mathf.Max(8, Mathf.RoundToInt(mySettings.BaseCircleSegments * Mathf.Abs(mySettings.Angle) / 360.0f));

            for (int i = 0; i < vertsCount; i++)
            {
                verts.Add(Vector3.Lerp(meshie.vertices[0], meshie.vertices[2], (i * 1.0f) / (vertsCount - 1)));
            }

            //curve the verts in canvas local space
            if (mySettings.Angle != 0)
            {
                Rect canvasRect = myCanvas.GetComponent<RectTransform>().rect;
                float radius = mySettings.GetCyllinderRadiusInCanvasSpace();

                for (int i = 0; i < verts.Count; i++)
                {

                    Vector3 newpos = verts[i];
                    if (vertical)
                    {
                        float theta = (verts[i].y / canvasRect.size.y) * mySettings.Angle * Mathf.Deg2Rad;
                        newpos.y = Mathf.Sin(theta) * radius;
                        newpos.z += Mathf.Cos(theta) * radius - radius;
                        verts[i] = newpos;
                    }
                    else
                    {
                        float theta = (verts[i].x / canvasRect.size.x) * mySettings.Angle * Mathf.Deg2Rad;
                        newpos.x = Mathf.Sin(theta) * radius;
                        newpos.z += Mathf.Cos(theta) * radius - radius;
                        verts[i] = newpos;
                    }
                }
            }


            //create our box colliders and arrange them in a nice cyllinder
            float boxDepth = mySettings.GetTesslationSize(false).x / 10;
            for (int i = 0; i < verts.Count - 1; i++)
            {
                GameObject newBox = new GameObject("Box collider");
                newBox.layer = this.gameObject.layer;
                newBox.transform.SetParent(go.transform);
                newBox.transform.ResetTransform();
                newBox.AddComponent<BoxCollider>();

                if (vertical)
                {
                    newBox.transform.localPosition = new Vector3(0, (verts[i + 1].y + verts[i].y) * 0.5f, (verts[i + 1].z + verts[i].z) * 0.5f);
                    newBox.transform.localScale = new Vector3(boxDepth, Vector3.Distance(Vertices[0], Vertices[1]), Vector3.Distance(verts[i + 1], verts[i]));
                    newBox.transform.localRotation = Quaternion.LookRotation((verts[i + 1] - verts[i]), Vertices[0] - Vertices[1]);
                }
                else
                {
                    newBox.transform.localPosition = new Vector3((verts[i + 1].x + verts[i].x) * 0.5f, 0, (verts[i + 1].z + verts[i].z) * 0.5f);
                    newBox.transform.localScale = new Vector3(boxDepth, Vector3.Distance(Vertices[0], Vertices[1]), Vector3.Distance(verts[i + 1], verts[i]));
                    newBox.transform.localRotation = Quaternion.LookRotation((verts[i + 1] - verts[i]), Vertices[0] - Vertices[1]);
                }

            }

            return go;

        }

        Mesh CreateCyllinderColliderMesh(bool vertical = false)
        {

            Mesh meshie = new Mesh();

            Vector3[] Vertices = new Vector3[4];
            (myCanvas.transform as RectTransform).GetWorldCorners(Vertices);
            meshie.vertices = Vertices;

            //rearrange them to be in an easy to interpolate order and convert to canvas local spce
            if (vertical)
            {
                Vertices[0] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[1]);
                Vertices[1] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[2]);
                Vertices[2] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[0]);
                Vertices[3] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[3]);
            }
            else
            {
                Vertices[0] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[1]);
                Vertices[1] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[0]);
                Vertices[2] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[2]);
                Vertices[3] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[3]);
            }

            meshie.vertices = Vertices;

            //create a new array of vertices, subdivided as needed
            List<Vector3> verts = new List<Vector3>();
            int vertsCount = Mathf.Max(8, Mathf.RoundToInt(mySettings.BaseCircleSegments * Mathf.Abs(mySettings.Angle) / 360.0f));


            for (int i = 0; i < vertsCount; i++)
            {
                verts.Add(Vector3.Lerp(meshie.vertices[0], meshie.vertices[2], (i * 1.0f) / (vertsCount - 1)));
                verts.Add(Vector3.Lerp(meshie.vertices[1], meshie.vertices[3], (i * 1.0f) / (vertsCount - 1)));
            }


            //curve the verts in canvas local space
            if (mySettings.Angle != 0)
            {
                Rect canvasRect = myCanvas.GetComponent<RectTransform>().rect;
                float radius = GetComponent<CurvedUISettings>().GetCyllinderRadiusInCanvasSpace();

                for (int i = 0; i < verts.Count; i++)
                {

                    Vector3 newpos = verts[i];
                    if (vertical)
                    {
                        float theta = (verts[i].y / canvasRect.size.y) * mySettings.Angle * Mathf.Deg2Rad;
                        newpos.y = Mathf.Sin(theta) * radius;
                        newpos.z += Mathf.Cos(theta) * radius - radius;
                        verts[i] = newpos;
                    }
                    else
                    {
                        float theta = (verts[i].x / canvasRect.size.x) * mySettings.Angle * Mathf.Deg2Rad;
                        newpos.x = Mathf.Sin(theta) * radius;
                        newpos.z += Mathf.Cos(theta) * radius - radius;
                        verts[i] = newpos;
                    }


                }
            }

            meshie.vertices = verts.ToArray();

            //create triangles drom verts
            List<int> tris = new List<int>();
            for (int i = 0; i < verts.Count / 2 - 1; i++)
            {
                if (vertical)
                {
                    //forward tris
                    tris.Add(i * 2 + 0);
                    tris.Add(i * 2 + 1);
                    tris.Add(i * 2 + 2);

                    tris.Add(i * 2 + 1);
                    tris.Add(i * 2 + 3);
                    tris.Add(i * 2 + 2);
                }
                else {
                    //forward tris
                    tris.Add(i * 2 + 2);
                    tris.Add(i * 2 + 1);
                    tris.Add(i * 2 + 0);

                    tris.Add(i * 2 + 2);
                    tris.Add(i * 2 + 3);
                    tris.Add(i * 2 + 1);
                }
            }
            meshie.triangles = tris.ToArray();

            return meshie;
        }

        Mesh CreateSphereColliderMesh()
        {

            Mesh meshie = new Mesh();

            Vector3[] Corners = new Vector3[4];
            (myCanvas.transform as RectTransform).GetWorldCorners(Corners);

            List<Vector3> verts = new List<Vector3>(Corners);
            for (int i = 0; i < verts.Count; i++)
            {
                verts[i] = mySettings.transform.worldToLocalMatrix.MultiplyPoint3x4(verts[i]);
            }

            if (mySettings.Angle != 0)
            {
                // Tesselate quads and apply transformation
                int startingVertexCount = verts.Count;
                for (int i = 0; i < startingVertexCount; i += 4)
                    ModifyQuad(verts, i, mySettings.GetTesslationSize(false));

                // Remove old quads
                verts.RemoveRange(0, startingVertexCount);

                //curve verts
                float vangle = mySettings.VerticalAngle;
                float cylinder_angle = mySettings.Angle;
                Vector2 canvasSize = (myCanvas.transform as RectTransform).rect.size;
                float radius = mySettings.GetCyllinderRadiusInCanvasSpace();

                //caluclate vertical angle for aspect - consistent mapping
                if (mySettings.PreserveAspect)
                {
                    vangle = mySettings.Angle * (canvasSize.y / canvasSize.x);
                }
                else {//if we're not going for constant aspect, set the width of the sphere to equal width of the original canvas
                    radius = canvasSize.x / 2.0f;
                }

                //curve the vertices 
                for (int i = 0; i < verts.Count; i++)
                {

                    float theta = (verts[i].x / canvasSize.x).Remap(-0.5f, 0.5f, (180 - cylinder_angle) / 2.0f - 90, 180 - (180 - cylinder_angle) / 2.0f - 90);
                    theta *= Mathf.Deg2Rad;
                    float gamma = (verts[i].y / canvasSize.y).Remap(-0.5f, 0.5f, (180 - vangle) / 2.0f, 180 - (180 - vangle) / 2.0f);
                    gamma *= Mathf.Deg2Rad;

                    verts[i] = new Vector3(Mathf.Sin(gamma) * Mathf.Sin(theta) * radius,
                        -radius * Mathf.Cos(gamma),
                        Mathf.Sin(gamma) * Mathf.Cos(theta) * radius + (mySettings.PreserveAspect ? -radius : 0));
                }
            }
            meshie.vertices = verts.ToArray();

            //create triangles from verts
            List<int> tris = new List<int>();
            for (int i = 0; i < verts.Count; i += 4)
            {
                tris.Add(i + 0);
                tris.Add(i + 1);
                tris.Add(i + 2);

                tris.Add(i + 3);
                tris.Add(i + 0);
                tris.Add(i + 2);
            }


            meshie.triangles = tris.ToArray();
            return meshie;
        }


        #endregion


        #region SUPPORT FUNCTIONS
        /// <summary>
        /// Determine the signed angle between two vectors, with normal 'n'
        /// as the rotation axis.
        /// </summary>
        float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
        {
            return Mathf.Atan2(
                Vector3.Dot(n, Vector3.Cross(v1, v2)),
                Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
        }

        private bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
        {
            if (!useDragThreshold)
                return true;

            return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
        }

        protected virtual void ProcessMove(PointerEventData pointerEvent)
        {
            var targetGO = pointerEvent.pointerCurrentRaycast.gameObject;
            HandlePointerExitAndEnter(pointerEvent, targetGO);
        }

        protected void UpdateSelectedObjects(PointerEventData eventData)
        {

            //deselect last object if we moved beyond it
            bool selectedStillUnderGaze = false;
            foreach (GameObject go in eventData.hovered)
            {
                if (go == eventData.selectedObject)
                {
                    selectedStillUnderGaze = true;
                    break;
                }
            }
            if (!selectedStillUnderGaze) eventData.selectedObject = null;


            //find new object to select in hovered objects
            foreach (GameObject go in eventData.hovered)
            {
                if (go == null) continue;

                //go through only go that can be selected and are drawn by the canvas
                Graphic gph = go.GetComponent<Graphic>();
#if UNITY_5_1
                        if (go.GetComponent<Selectable>() != null && gph != null && gph.depth != -1)
#else
                if (go.GetComponent<Selectable>() != null && gph != null && gph.depth != -1 && gph.raycastTarget)
#endif
                {
                    if (eventData.selectedObject != go)
                        eventData.selectedObject = go;
                    break;
                }
            }


            if (mySettings.ControlMethod == CurvedUIInputModule.CUIControlMethod.GAZE)
            {
                //Test for selected object being dragged and initialize dragging, if needed.
                //We do this here to trick unity's StandAloneInputModule into thinking we used a touch or mouse to do it.
                if (eventData.IsPointerMoving() && eventData.pointerDrag != null
                    && !eventData.dragging
                    && ShouldStartDrag(eventData.pressPosition, eventData.position, EventSystem.current.pixelDragThreshold, eventData.useDragThreshold))
                {
                    ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.beginDragHandler);
                    eventData.dragging = true;
                }
            }
        }

        // walk up the tree till a common root between the last entered and the current entered is foung
        // send exit events up to (but not inluding) the common root. Then send enter events up to
        // (but not including the common root).
        protected void HandlePointerExitAndEnter(PointerEventData currentPointerData, GameObject newEnterTarget)
        {
            // if we have no target / pointerEnter has been deleted
            // just send exit events to anything we are tracking
            // then exit
            if (newEnterTarget == null || currentPointerData.pointerEnter == null)
            {
                for (var i = 0; i < currentPointerData.hovered.Count; ++i)
                    ExecuteEvents.Execute(currentPointerData.hovered[i], currentPointerData, ExecuteEvents.pointerExitHandler);

                currentPointerData.hovered.Clear();

                if (newEnterTarget == null)
                {
                    currentPointerData.pointerEnter = newEnterTarget;
                    return;
                }
            }

            // if we have not changed hover target
            if (currentPointerData.pointerEnter == newEnterTarget && newEnterTarget)
                return;

            GameObject commonRoot = FindCommonRoot(currentPointerData.pointerEnter, newEnterTarget);

            // and we already an entered object from last time
            if (currentPointerData.pointerEnter != null)
            {
                // send exit handler call to all elements in the chain
                // until we reach the new target, or null!
                Transform t = currentPointerData.pointerEnter.transform;

                while (t != null)
                {
                    // if we reach the common root break out!
                    if (commonRoot != null && commonRoot.transform == t)
                        break;

                    ExecuteEvents.Execute(t.gameObject, currentPointerData, ExecuteEvents.pointerExitHandler);
                    currentPointerData.hovered.Remove(t.gameObject);
                    t = t.parent;
                }
            }

            // now issue the enter call up to but not including the common root
            currentPointerData.pointerEnter = newEnterTarget;
            if (newEnterTarget != null)
            {
                Transform t = newEnterTarget.transform;

                while (t != null && t.gameObject != commonRoot)
                {
                    ExecuteEvents.Execute(t.gameObject, currentPointerData, ExecuteEvents.pointerEnterHandler);
                    currentPointerData.hovered.Add(t.gameObject);
                    t = t.parent;
                }
            }
        }

        protected static GameObject FindCommonRoot(GameObject g1, GameObject g2)
        {
            if (g1 == null || g2 == null)
                return null;

            var t1 = g1.transform;
            while (t1 != null)
            {
                var t2 = g2.transform;
                while (t2 != null)
                {
                    if (t1 == t2)
                        return t1.gameObject;
                    t2 = t2.parent;
                }
                t1 = t1.parent;
            }
            return null;
        }

        /// <summary>
        /// REturns a screen point under which a ray intersects the curved canvas in its event camera view
        /// </summary>
        /// <returns><c>true</c>, if screen space point by ray was gotten, <c>false</c> otherwise.</returns>
        /// <param name="ray">Ray.</param>
        /// <param name="o_positionOnCanvas">O position on canvas.</param>
        bool GetScreenSpacePointByRay(Ray ray, out Vector2 o_positionOnCanvas)
        {

            switch (mySettings.Shape)
            {
                case CurvedUISettings.CurvedUIShape.CYLINDER:
                {
                    return RaycastToCyllinderCanvas(ray, out o_positionOnCanvas, false);
                }
                case CurvedUISettings.CurvedUIShape.CYLINDER_VERTICAL:
                {
                    return RaycastToCyllinderVerticalCanvas(ray, out o_positionOnCanvas, false);
                }
                case CurvedUISettings.CurvedUIShape.RING:
                {
                    return RaycastToRingCanvas(ray, out o_positionOnCanvas, false);
                }
                case CurvedUISettings.CurvedUIShape.SPHERE:
                {
                    return RaycastToSphereCanvas(ray, out o_positionOnCanvas, false);
                }
                default:
                {
                    o_positionOnCanvas = Vector2.zero;
                    return false;
                }
            }

        }
        #endregion





        #region PUBLIC

        /// <summary>
        /// Returns true if user's pointer is currently pointing inside this canvas.
        /// </summary>
        public bool PointingAtCanvas {
            get { return pointingAtCanvas; }
        }

        public void RebuildCollider()
        {
            cyllinderMidPoint = new Vector3(0, 0, -mySettings.GetCyllinderRadiusInCanvasSpace());
            CreateCollider();
        }

        /// <summary>
        /// Returns all objects currently under the pointer
        /// </summary>
        /// <returns>The objects under pointer.</returns>
        public List<GameObject> GetObjectsUnderPointer()
        {
            if (objectsUnderPointer == null) objectsUnderPointer = new List<GameObject>();
            return objectsUnderPointer;
        }


        /// <summary>
        /// Returns all the canvas objects that are visible under given Screen Position of EventCamera
        /// </summary>
        public List<GameObject> GetObjectsUnderScreenPos(Vector2 screenPos, Camera eventCamera = null)
        {
            if (eventCamera == null)
                eventCamera = myCanvas.worldCamera;

            return GetObjectsHitByRay(eventCamera.ScreenPointToRay(screenPos));
        }

        /// <summary>
        /// Returns all the canvas objects that are intersected by given ray
        /// </summary>
        /// <returns>The objects hit by ray.</returns>
        /// <param name="ray">Ray.</param>
        public List<GameObject> GetObjectsHitByRay(Ray ray)
        {
            List<GameObject> results = new List<GameObject>();

            Vector2 pointerPosition;

            //ray outside the canvas, return null
            if (!GetScreenSpacePointByRay(ray, out pointerPosition))
                return results;

            //lets find the graphics under ray!
            List<Graphic> s_SortedGraphics = new List<Graphic>();
            var foundGraphics = GraphicRegistry.GetGraphicsForCanvas(myCanvas);
            for (int i = 0; i < foundGraphics.Count; ++i)
            {
                Graphic graphic = foundGraphics[i];

                // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
                if (graphic.depth == -1 || !graphic.raycastTarget)
                    continue;

                if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPosition, eventCamera))
                    continue;

                if (graphic.Raycast(pointerPosition, eventCamera))
                    s_SortedGraphics.Add(graphic);

            }

            s_SortedGraphics.Sort((g1, g2) => g2.depth.CompareTo(g1.depth));
            for (int i = 0; i < s_SortedGraphics.Count; ++i)
                results.Add(s_SortedGraphics[i].gameObject);

            s_SortedGraphics.Clear();

            return results;
        }

        /// <summary>
        /// Sends OnClick event to every Button under pointer.
        /// </summary>
        public void Click()
        {
            for (int i = 0; i < GetObjectsUnderPointer().Count; i++)
            {
                if (GetObjectsUnderPointer()[i].GetComponent<Slider>())//slider requires a diffrent event to click.
                {
                    ExecuteEvents.Execute(GetObjectsUnderPointer()[i], lastFrameEventData, ExecuteEvents.pointerDownHandler); 
                    ExecuteEvents.Execute(GetObjectsUnderPointer()[i], lastFrameEventData, ExecuteEvents.pointerUpHandler);
                }
                else
                    ExecuteEvents.Execute(GetObjectsUnderPointer()[i], new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler); //all other ui objects
            }
        }
        #endregion



        #region TESSELATION
        void ModifyQuad(List<Vector3> verts, int vertexIndex, Vector2 requiredSize)
        {

            // Read the existing quad vertices
            List<Vector3> quad = new List<Vector3>();
            for (int i = 0; i < 4; i++)
                quad.Add(verts[vertexIndex + i]);

            // horizotal and vertical directions of a quad. We're going to tesselate parallel to these.
            Vector3 horizontalDir = quad[2] - quad[1];
            Vector3 verticalDir = quad[1] - quad[0];

            // Find how many quads we need to create
            int horizontalQuads = Mathf.CeilToInt(horizontalDir.magnitude * (1.0f / Mathf.Max(1.0f, requiredSize.x)));
            int verticalQuads = Mathf.CeilToInt(verticalDir.magnitude * (1.0f / Mathf.Max(1.0f, requiredSize.y)));

            // Create the quads!
            float yStart = 0.0f;
            for (int y = 0; y < verticalQuads; ++y)
            {

                float yEnd = (y + 1.0f) / verticalQuads;
                float xStart = 0.0f;

                for (int x = 0; x < horizontalQuads; ++x)
                {
                    float xEnd = (x + 1.0f) / horizontalQuads;

                    //Add new quads to list
                    verts.Add(TesselateQuad(quad, xStart, yStart));
                    verts.Add(TesselateQuad(quad, xStart, yEnd));
                    verts.Add(TesselateQuad(quad, xEnd, yEnd));
                    verts.Add(TesselateQuad(quad, xEnd, yStart));

                    //begin the next quad where we ened this one
                    xStart = xEnd;
                }
                //begin the next row where we ended this one
                yStart = yEnd;
            }
        }


        Vector3 TesselateQuad(List<Vector3> quad, float x, float y)
        {

            Vector3 ret = Vector3.zero;

            //1. calculate weighting factors
            List<float> weights = new List<float>(){
                (1-x) * (1-y),
                (1-x) * y,
                x * y,
                x * (1-y),
            };

            //2. interpolate pos using weighting factors
            for (int i = 0; i < 4; i++)
                ret += quad[i] * weights[i];
            return ret;
        }

        #endregion

    }
}
