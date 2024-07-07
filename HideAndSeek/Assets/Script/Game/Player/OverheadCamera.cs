using UnityEngine;

/// <summary>
/// ��󎋓_�J�����̏���
/// </summary>
public class OverheadCamera : MonoBehaviour
{
    #region PrivateField
    /// <summary>�J�����̈ړ����x</summary>
    private float moveSpeed = 10f;
    #endregion

    #region UnityEvent
    private void Update()
    {
        MoveCamera();
    }
    #endregion

    #region PrivateMethod
    /// <summary>
    /// �J�����̈ړ����������郁�\�b�h
    /// </summary>
    private void MoveCamera()
    {
        // ���͂��琅������ѐ��������̈ړ��ʂ��擾
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // ���͕������x�N�g���Ƃ��Ē�`
        Vector3 direction = new Vector3(horizontal, 0, vertical);

        // �J�������ړ�������
        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
    }
    #endregion
}
