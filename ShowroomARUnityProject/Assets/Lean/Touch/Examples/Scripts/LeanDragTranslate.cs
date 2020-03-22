using UnityEngine;

namespace Lean.Touch
{
	/// <summary>This component allows you to translate the current GameObject relative to the camera using the finger drag gesture.</summary>
	[HelpURL(LeanTouch.HelpUrlPrefix + "LeanDragTranslate")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Drag Translate")]
	public class LeanDragTranslate : MonoBehaviour
	{
		/// <summary>The method used to find fingers to use with this component. See LeanFingerFilter documentation for more information.</summary>
		public LeanFingerFilter Use = new LeanFingerFilter(true);

		/// <summary>The camera the translation will be calculated using.\n\nNone = MainCamera.</summary>
		[Tooltip("The camera the translation will be calculated using.\n\nNone = MainCamera.")]
		public Camera Camera;

		/// <summary>If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.
		/// -1 = Instantly change.
		/// 1 = Slowly change.
		/// 10 = Quickly change.</summary>
		[Tooltip("If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.\n\n-1 = Instantly change.\n\n1 = Slowly change.\n\n10 = Quickly change.")]
		public float Dampening = -1.0f;

		[HideInInspector]
		[SerializeField]
		private Vector3 remainingTranslation;

		/// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually add a finger.</summary>
		public void AddFinger(LeanFinger finger)
		{
			Use.AddFinger(finger);
		}

		/// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually remove a finger.</summary>
		public void RemoveFinger(LeanFinger finger)
		{
			Use.RemoveFinger(finger);
		}

		/// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually remove all fingers.</summary>
		public void RemoveAllFingers()
		{
			Use.RemoveAllFingers();
		}
#if UNITY_EDITOR
		protected virtual void Reset()
		{
			Use.UpdateRequiredSelectable(gameObject);
		}
#endif
		protected virtual void Awake()
		{
			Use.UpdateRequiredSelectable(gameObject);
		}

		protected virtual void Update()
		{
			// Store
			var oldPosition = transform.localPosition;

			// Get the fingers we want to use
			var fingers = Use.GetFingers();

			// Calculate the screenDelta value based on these fingers
			var screenDelta = LeanGesture.GetScreenDelta(fingers);

			if (screenDelta != Vector2.zero)
			{
				// Perform the translation
				if (transform is RectTransform)
				{
					TranslateUI(screenDelta);
				}
				else
				{
					Translate(screenDelta);
				}
			}

			// Increment
			remainingTranslation += transform.localPosition - oldPosition;

			// Get t value
			var factor = LeanTouch.GetDampenFactor(Dampening, Time.deltaTime);

			// Dampen remainingDelta
			var newRemainingTranslation = Vector3.Lerp(remainingTranslation, Vector3.zero, factor);

			// Shift this transform by the change in delta
			transform.localPosition = oldPosition + remainingTranslation - newRemainingTranslation;

			// Update remainingDelta with the dampened value
			remainingTranslation = newRemainingTranslation;

		}

		private void TranslateUI(Vector2 screenDelta)
		{
			var camera = Camera;

			if (camera == null)
			{
				var canvas = transform.GetComponentInParent<Canvas>();

				if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
				{
					camera = canvas.worldCamera;
				}
			}

			// Screen position of the transform
			var screenPoint = RectTransformUtility.WorldToScreenPoint(camera, transform.position);

			// Add the deltaPosition
			screenPoint += screenDelta;

			// Convert back to world space
			var worldPoint = default(Vector3);

			if (RectTransformUtility.ScreenPointToWorldPointInRectangle(transform.parent as RectTransform, screenPoint, camera, out worldPoint) == true)
			{
				transform.position = worldPoint;
			}
		}

		private void Translate(Vector2 screenDelta)
		{
			// Make sure the camera exists
			var camera = LeanTouch.GetCamera(Camera, gameObject);

			if (camera != null)
			{
				// Screen position of the transform
				var screenPoint = camera.WorldToScreenPoint(transform.position);

				// Add the deltaPosition
				Vector3 XZDelta = (Vector3)screenDelta;
				XZDelta.y = 0;
				screenPoint += XZDelta;

				// Convert back to world space
				transform.position = camera.ScreenToWorldPoint(screenPoint);
			}
			else
			{
				Debug.LogError("Failed to find camera. Either tag your camera as MainCamera, or set one in this component.", this);
			}
		}
	}
}


/*
using UnityEngine;
namespace Lean.Touch
{
    // This script allows you to transform the current GameObject with smoothing
    public class LeanDragTranslate : MonoBehaviour
    {
        [Tooltip("How smoothly this object moves to its target position")]
        public float Dampening = 10.0f;
        // The position we still need to add
        [HideInInspector]
        public Vector3 RemainingDelta;
        protected virtual void LateUpdate()
        {
            // Get t value
            var factor = LeanTouch.GetDampenFactor(Dampening, Time.deltaTime);
            // Dampen remainingDelta
            var newDelta = Vector3.Lerp(RemainingDelta, Vector3.zero, factor);
            // Shift this transform by the change in delta(Atul`s change)
            Vector3 moveMe = (RemainingDelta - newDelta);
            moveMe.z = moveMe.y;
            moveMe.y = 0f;
            Debug.Log("move me : " + moveMe);
            //transform.position += (RemainingDelta - newDelta);
            transform.position += moveMe;
           // Debug.Log("pos : "+transform.position);
            // Update remainingDelta with the dampened value
            RemainingDelta = newDelta;
        }
        protected override void Translate(Vector2 screenDelta)
        {
            Debug.Log("HI!!!!SSSS");
            // Make sure the camera exists
            var camera = LeanTouch.GetCamera(Camera, gameObject);
            if (camera != null)
            {
                // Store old position
                var oldPosition = transform.position;
                // Screen position of the transform
                var screenPosition = camera.WorldToScreenPoint(oldPosition);
                // Add the deltaPosition
                screenPosition += (Vector3)screenDelta;
                // Convert back to world space
                var newPosition = camera.ScreenToWorldPoint(screenPosition);
                // Add to delta
                RemainingDelta += newPosition - oldPosition;
            }
        }
    }
}
*/