using UnityEngine;

/// <summary>
/// エフェクト終了時に消滅させる処理
/// </summary>
public class DestroyOnParticleSystemEnd : MonoBehaviour
{
    #region PrivateField
    /// <summary>パーティクルシステム</summary>
    private ParticleSystem particleSystem;
    #endregion

    #region UnityEvent
    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        
        // パーティクルシステムがnullの場合、直ぐに消滅させる
        if (particleSystem == null)
        {
            Destroy(this);
        }
    }

    void Update()
    {
        if (particleSystem != null && !particleSystem.IsAlive())
        {
            Destroy(gameObject);
        }
    }
    #endregion
}