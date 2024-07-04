using UnityEngine;

namespace Player
{
    public enum PlayerMoveStatus
    {
        idol,
        walk,
    }

    public class PlayerController : MonoBehaviour
    {
        #region PrivateField
        private PlayerMoveStatus playerMoveStatus;
        #endregion

        #region SerializeField
        [SerializeField] private float speed = 5f;
        #endregion

        #region UnityEvent
        private void Start()
        {
            //animator = GetComponent<Animator>();
        }

        private void FixedUpdate()
        {
            PlayerMove();
        }
        #endregion

        #region PrivateMethod
        private void PlayerMove()
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ||
                Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ||
                Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ||
                Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                playerMoveStatus = PlayerMoveStatus.walk;
            }
            else
            {
                playerMoveStatus = PlayerMoveStatus.idol;
            }

            switch (playerMoveStatus)
            {
                case PlayerMoveStatus.idol:
                    PlayerIdol();
                    break;
                case PlayerMoveStatus.walk:
                    PlayerWalk();
                    break;
            }
        }

        private void PlayerIdol()
        {
            //animator.SetBool("walk", false);
        }

        private void PlayerWalk()
        {
            //animator.SetBool("walk", true);

            float horizontal = Input.GetAxis("Horizontal") * speed;
            float vertical = Input.GetAxis("Vertical") * speed;

            //Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
            transform.position += transform.forward * vertical + transform.right * horizontal;
        }
        #endregion
    }
}