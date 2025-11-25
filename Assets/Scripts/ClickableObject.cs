using UnityEngine;
using System.Collections; // Coroutine을 사용하기 위해 필요합니다.

/// <summary>
/// 클릭 가능한 오브젝트에 부착되어, 소리를 재생한 후 지연 파괴를 처리합니다.
/// GameManager에서 이 오브젝트를 직접 파괴하는 대신 이 시퀀스를 호출합니다.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class ClickableObject : MonoBehaviour
{
    // Inspector에서 할당할 야옹 소리 오디오 클립
    [Tooltip("오브젝트 클릭 시 재생할 소리 클립입니다.")]
    public AudioClip MeowSound;

    private AudioSource audioSource;
    private Collider objectCollider;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        objectCollider = GetComponent<Collider>();
    }

    /// <summary>
    /// GameManager에서 호출되어 소리 재생 및 파괴 시퀀스를 시작합니다.
    /// </summary>
    public void StartDestructionSequence()
    {
        // 1. 소리가 재생되는 동안 추가 클릭 방지
        if (objectCollider != null)
        {
            objectCollider.enabled = false;
        }

        // 2. 시각적 변화 (선택 사항: 클릭되면 사라지거나 튀어나오는 등의 효과)
        // 예: GetComponent<MeshRenderer>().enabled = false;

        // 3. 소리 재생
        if (MeowSound != null)
        {
            audioSource.PlayOneShot(MeowSound);

            // 4. 소리 재생이 끝날 때까지 기다린 후 파괴 코루틴 시작
            StartCoroutine(DestroyAfterSound(MeowSound.length));
        }
        else
        {
            // 소리가 없다면 즉시 파괴
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 지정된 시간만큼 기다린 후 오브젝트를 파괴하는 코루틴입니다.
    /// </summary>
    /// <param name="delay">파괴할 때까지 기다릴 시간 (초)</param>
    IEnumerator DestroyAfterSound(float delay)
    {
        // 소리가 끝날 때까지 기다립니다.
        yield return new WaitForSeconds(delay);

        // 파괴
        Destroy(gameObject);
    }
}