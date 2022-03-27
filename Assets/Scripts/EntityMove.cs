/********************************************************************
	created:	2022/03/25
	filename: 	EntityMove.cs
	file path:	Assets\Scripts\
	file ext:	cs
	author:		yungang
	
	purpose:    测试摇杆
*********************************************************************/

using UnityEngine;

namespace ST.UI {
    public class EntityMove : MonoBehaviour {
        public Joystick mJoystick = null;
        public float mSpeed = 5.0f;
        public GameObject mRoleObj = null;

        private Animator mAnimator = null;
        private CharacterController mController = null;

        void Start() {
            mAnimator = mRoleObj.GetComponent<Animator>();
            mController = transform.GetComponent<CharacterController>();

            mJoystick.mOnJoystickDragEvent.AddListener(v => {
                if (v.magnitude != 0) {
                    mAnimator.SetBool("RunState", true);
                    Vector3 direction = new Vector3(v.x, 0, v.y);
                    mController.Move(direction * mSpeed * Time.deltaTime);
                    mRoleObj.transform.rotation = Quaternion.LookRotation(new Vector3(v.x, 0, v.y));
                }
            });

            mJoystick.mOnJoystickTouchDownEvent.AddListener(v => {
                mAnimator.SetBool("RunState", true);
            });

            mJoystick.mOnJoystickTouchUpEvent.AddListener(v => {
                mAnimator.SetBool("RunState", false);
            });
        }
    }
}
