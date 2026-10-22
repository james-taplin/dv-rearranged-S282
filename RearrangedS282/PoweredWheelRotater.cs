using UnityEngine;

namespace RearrangedS282
{
	// The native Animator drives the rods and original wheels. Added fifth/sixth
	// wheels must copy its completed pose, not the previous frame's Update pose.
	internal class PoweredWheelRotater : MonoBehaviour
	{
		//Wheel whose position we'll match
		private GameObject coupledWheel;

		internal GameObject Source => coupledWheel;

		// A cloned drivetrain must follow its own native fourth wheel. Unity's
		// copied component references are not a reason to keep native port readers.
		internal static void BindClonedDrive(Transform drive)
		{
			var source = drive.Find("s282_wheels_driving_4");
			foreach (var follower in drive.GetComponentsInChildren<PoweredWheelRotater>(true))
			{
				if (source == null) throw new System.InvalidOperationException("Cloned drivetrain has no native rotation source.");
				follower.Init(source.gameObject);
				follower.enabled = true;
			}
		}

		public void Init(GameObject coupledWheel)
		{
			this.coupledWheel = coupledWheel;
		}

		public void LateUpdate()
		{
			if (coupledWheel != null)
			{
				transform.localRotation = coupledWheel.transform.localRotation;
			}
		}

	}
}
