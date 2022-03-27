/********************************************************************
	created:	2022/03/25
	filename: 	Joystick.cs
	file path:	Assets\Scripts\Joystick\
	file ext:	cs
	author:		yungang
	
	purpose:    UGUI 摇杆
*********************************************************************/

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace ST.UI {
    // 首先定义摇杆事件回调以及所携带的数据;
    public class JoystickEvent : UnityEvent<Vector2> {};

    // 定义摇杆类，并继承 Down、Drag、Up 以处理对应的事件逻辑;
    public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler {
        #region Public Property
        public float mRadius = 38; // 摇杆移动半径;
        public bool mIsDyJoystick = true; // 是否为动态摇杆(其实就是点击屏幕的哪个位置，哪个位置就显示这个摇杆并随之进行摇杆操作类似【王者荣耀】);
        public bool mShowDir = true; // 是否显示摇杆方向箭(类似【王者荣耀】);

        public Transform mHandleRockerBallTrans; // 摇杆头;
        public Transform mAreaTrans; // 摇杆背景;
        public Transform mDirTrans; // 摇杆方向指示器;
        #endregion

        #region HideInInspector Public Property
        [HideInInspector] public JoystickEvent mOnJoystickDragEvent = new JoystickEvent(); // 摇杆拖动事件回调（这里也可以使用 Delegate）;
        [HideInInspector] public JoystickEvent mOnJoystickTouchDownEvent = new JoystickEvent(); // 摇杆按下事件回调（这里也可以使用 Delegate）;
        [HideInInspector] public JoystickEvent mOnJoystickTouchUpEvent = new JoystickEvent(); // 摇杆抬起事件回调（这里也可以使用 Delegate）;
        #endregion

        #region Private Property
        private int mFingerId = int.MinValue; // 主要考虑多指操作，默认设置一个暂时无用的数值;
        private Vector3 mBkgOriginLocalPos; // 缓存背景图的原始位置;
        private Vector3 mTouchDownPos; // 手指按下的位;
        #endregion

        #region System Func
        private void Start() {
            mBkgOriginLocalPos = mAreaTrans.localPosition;  // 首先记录初始位置;
        }

        private void FixedUpdate() {
            mOnJoystickDragEvent.Invoke( mHandleRockerBallTrans.localPosition / mRadius ); // FixedUpdate 中处理拖动事件（后面主要考虑有相机随着移动，测试的话可以直接在 Update 中操作）;
        }

        private void OnDisable() {
            RestAll();  // 当这个脚本失效时需要处理所有的信息恢复初始状态;
        }
        #endregion

        #region Event Handler Interface
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
            if (eventData.pointerId < -1 || mFingerId != int.MinValue) return; // 目前指处理单指并且不能在拖拽状态被处理;
            mFingerId = eventData.pointerId;
            mTouchDownPos = eventData.position;
            if (mIsDyJoystick) {
                mTouchDownPos[2] = eventData.pressEventCamera?.WorldToScreenPoint(mAreaTrans.position).z ?? mAreaTrans.position.z;
                mAreaTrans.position = eventData.pressEventCamera?.ScreenToWorldPoint(mTouchDownPos) ?? mTouchDownPos;
            }
            mOnJoystickTouchDownEvent.Invoke(eventData.position); // 处理摇杆被按下的事件;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (mFingerId != eventData.pointerId) return;
            Vector2 vDirVector = eventData.position - (Vector2)mTouchDownPos; // 由于摇杆头是摇杆背景的子节点，这个差是点触位置到摇杆背景的向量（注：需要关注锚点如何设置）;
            float fRealRadius = Mathf.Clamp(Vector3.Magnitude(vDirVector), 0, mRadius); // 拖动的最大半径不能超过设置的半径;
            Vector2 vLocalPos = new Vector2((vDirVector.normalized * fRealRadius).x, (vDirVector.normalized * fRealRadius).y);
            mHandleRockerBallTrans.localPosition = vLocalPos; // 摇杆头位置被控制在规定半径内;
            if (mShowDir) {
                if (!mDirTrans.gameObject.activeInHierarchy) mDirTrans.gameObject.SetActive(true);
                mDirTrans.localEulerAngles = new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, vLocalPos));
            }
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (mFingerId != eventData.pointerId) return;
            RestAll();
            mOnJoystickTouchUpEvent.Invoke(eventData.position); // 处理摇杆被抬起的事件;
        }
        #endregion

        #region Private Func
        private void RestAll() 
        {
            mAreaTrans.localPosition = mBkgOriginLocalPos; // 摇杆背景恢复默认位置;
            mHandleRockerBallTrans.localPosition = Vector3.zero; // 摇杆头恢复默认位置;
            mDirTrans.gameObject.SetActive(false); // 隐藏指示器;
            mFingerId = int.MinValue; // 记录手指信息恢复默认数值;
        } 
        #endregion
    }
}