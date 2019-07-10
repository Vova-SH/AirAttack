using UnityEngine;
using System.Collections;

namespace CurvedUI {

    /// <summary>
    /// This script switches the hand controlling the UI when a click on the other controller's trigger is detected.
    /// This emulates the functionality seen in SteamVR overlay or Oculus Home.
    /// Works both for SteamVR and Oculus SDK.
    /// </summary>
    public class CUI_ControllerHandSwitcher : MonoBehaviour
    {

        [SerializeField]
        GameObject LaserPointer;




#if CURVEDUI_TOUCH
        bool initialized = false;

        void Update()
        {
            if (!initialized)
            {
                //Launch Hand Switch. This will place the laser pointer in the current hand.
                //Works both for Oculus Touch and GearVR
                SwitchHandTo(CurvedUIInputModule.Instance.UsedHand);

                initialized = true;
            }

            //switch hand if user clicks with different one
            //This only works with OculusTouch
            if (OVRInput.GetDown(CurvedUIInputModule.Instance.OculusTouchInteractionButton, OVRInput.Controller.LTouch) && CurvedUIInputModule.Instance.UsedHand != CurvedUIInputModule.Hand.Left)
            {
               SwitchHandTo(CurvedUIInputModule.Hand.Left);
            }
            else if (OVRInput.GetDown(CurvedUIInputModule.Instance.OculusTouchInteractionButton, OVRInput.Controller.RTouch) && CurvedUIInputModule.Instance.UsedHand != CurvedUIInputModule.Hand.Right)
            {
               SwitchHandTo(CurvedUIInputModule.Hand.Right);
            }
        }

        void SwitchHandTo(CurvedUIInputModule.Hand newHand)
        {
            CurvedUIInputModule.Instance.UsedHand = newHand;
            LaserPointer.transform.SetParent(CurvedUIInputModule.Instance.OculusTouchUsedControllerTransform);
            LaserPointer.transform.ResetTransform();
        }


#elif CURVEDUI_VIVE
        void Start()
        {
            //connect to steamVR's OnModelLoaded events so we can update the pointer the moment controller is detected.
            CurvedUIInputModule.Right.ModelLoaded += OnModelLoaded;
            CurvedUIInputModule.Left.ModelLoaded += OnModelLoaded;
        }

        void OnModelLoaded(object sender)
        {
            SwitchHandTo(CurvedUIInputModule.Instance.UsedHand);
        }

        void Update()
        {
            if (CurvedUIInputModule.Right != null && CurvedUIInputModule.Right.IsTriggerDown && CurvedUIInputModule.Instance.UsedHand != CurvedUIInputModule.Hand.Right)
            {
                SwitchHandTo(CurvedUIInputModule.Hand.Right);

            }
            else if (CurvedUIInputModule.Left != null && CurvedUIInputModule.Left.IsTriggerDown && CurvedUIInputModule.Instance.UsedHand != CurvedUIInputModule.Hand.Left)
            {
                SwitchHandTo(CurvedUIInputModule.Hand.Left);

            }
        }

        void SwitchHandTo(CurvedUIInputModule.Hand newHand)
        {
            if (newHand == CurvedUIInputModule.Hand.Right)
            {
                CurvedUIInputModule.Instance.UsedHand = CurvedUIInputModule.Hand.Right;
                LaserPointer.transform.SetParent(CurvedUIInputModule.Right.transform);
                LaserPointer.transform.ResetTransform();
                LaserPointer.transform.position = CurvedUIInputModule.Right.PointingOrigin;
                LaserPointer.transform.LookAt(LaserPointer.transform.position + CurvedUIInputModule.Right.PointingDirection);
            }
            else
            {
                CurvedUIInputModule.Instance.UsedHand = CurvedUIInputModule.Hand.Left;
                LaserPointer.transform.SetParent(CurvedUIInputModule.Left.transform);
                LaserPointer.transform.ResetTransform();
                LaserPointer.transform.position = CurvedUIInputModule.Left.PointingOrigin;
                LaserPointer.transform.LookAt(LaserPointer.transform.position + CurvedUIInputModule.Left.PointingDirection);
            }
        }
#endif

    }

}


