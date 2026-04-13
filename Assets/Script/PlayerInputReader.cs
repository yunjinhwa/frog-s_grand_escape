using UnityEngine;

namespace SmallScaleInteractive._2DCharacter
{
    public class PlayerInputReader : MonoBehaviour
    {
        public bool LeftPressed => Input.GetKey(KeyCode.A);
        public bool RightPressed => Input.GetKey(KeyCode.D);
        public bool CrouchDown => Input.GetKeyDown(KeyCode.C);
        public bool CrouchUp => Input.GetKeyUp(KeyCode.C);
        public bool JumpPressed => Input.GetKeyDown(KeyCode.Space);
        public bool SlamPressed => Input.GetKeyDown(KeyCode.S);
        public bool DashPressed => Input.GetMouseButtonDown(1);
        public bool AttackPressed => Input.GetMouseButtonDown(0);
        public bool SlideDown => Input.GetKeyDown(KeyCode.LeftShift);
        public bool SlideUp => Input.GetKeyUp(KeyCode.LeftShift);
        public bool Skill1Pressed => Input.GetKeyDown(KeyCode.Alpha1);
        public bool Skill2Pressed => Input.GetKeyDown(KeyCode.Alpha2);
        public bool DamagePressed => Input.GetKeyDown(KeyCode.F);
        public bool DeathPressed => Input.GetKeyDown(KeyCode.V);
        public bool UpHeld => Input.GetKey(KeyCode.W);

        public float Horizontal => Input.GetAxisRaw("Horizontal");
        public float Vertical => Input.GetAxisRaw("Vertical");
    }
}