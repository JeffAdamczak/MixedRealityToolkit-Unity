﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace HoloToolkit.Unity.InputModule
{
    /// <summary>
    /// Waits for a controller to be instantiated, then attaches itself to a specified element
    /// </summary>
    public class AttachToController : ControllerFinder
    {
        public bool SetChildrenInactiveWhenDetached = true;

        [SerializeField]
        protected Vector3 PositionOffset = Vector3.zero;

        [SerializeField]
        protected Vector3 RotationOffset = Vector3.zero;

        [SerializeField]
        protected Vector3 ScaleOffset = Vector3.one;

        [SerializeField]
        protected bool SetScaleOnAttach = false;

        public bool IsAttached { get; private set; }

        protected virtual void OnAttachToController() { }
        protected virtual void OnDetachFromController() { }

        protected override void OnEnable()
        {
            SetChildrenActive(false);

            base.OnEnable();
        }

        protected override void AddControllerTransform(MotionControllerInfo newController)
        {
#if UNITY_WSA && UNITY_2017_2_OR_NEWER
            if (!IsAttached && newController.Handedness == Handedness)
            {
                base.AddControllerTransform(newController);

                // Parent ourselves under the element and set our offsets
                transform.parent = ElementTransform;
                transform.localPosition = PositionOffset;
                transform.localEulerAngles = RotationOffset;

                if (SetScaleOnAttach)
                {
                    transform.localScale = ScaleOffset;
                }

                SetChildrenActive(true);

                // Announce that we're attached
                OnAttachToController();

                IsAttached = true;
            }
#endif
        }

        protected override void RemoveControllerTransform(MotionControllerInfo oldController)
        {
#if UNITY_WSA && UNITY_2017_2_OR_NEWER
            if (IsAttached && oldController.Handedness == Handedness)
            {
                base.RemoveControllerTransform(oldController);

                OnDetachFromController();

                SetChildrenActive(false);

                transform.parent = null;

                IsAttached = false;
            }
#endif
        }

        private void SetChildrenActive(bool isActive)
        {
            if (SetChildrenInactiveWhenDetached)
            {
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(isActive);
                }
            }
        }
        void Reset()
        {
            // We want the default value of Handedness of Controller finders to be Unknown so it doesn't attach to random object.
            // But we also want the Editor to start with a useful default, so we set a Left handedness on inspector reset.
            Handedness = UnityEngine.XR.WSA.Input.InteractionSourceHandedness.Left;
        }
    }
}