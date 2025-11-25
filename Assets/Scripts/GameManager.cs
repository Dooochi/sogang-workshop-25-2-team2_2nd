using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI; // Image 컴포넌트 사용을 위해 추가
using TMPro; // 🌟 TextMeshPro 사용을 위해 필수 추가

public class GameManager : MonoBehaviour
{
    // 싱글턴 인스턴스
    public static GameManager Instance { get; private set; }

    // 게임 상태
    public enum GameState
    {
        Starting, // 게임 시작 중
        Playing,  // 플레이 중
        Paused,   // 일시정지
        GameOver  // 게임 오버
    }
    public GameState CurrentGameState { get; private set; }

    // 게임 데이터
    private int score = 0;
    public int Score { get { return score; } set { score = value; Debug.Log("[GameManager] 점수 업데이트: " + score); } }

    // 🌟 ▼ 유저 요청: 남은 오브젝트 수 표시용 UI 및 변수 ▼ 🌟
    [Header("UI Settings")]
    [Tooltip("남은 오브젝트 수를 표시할 TextMeshProUGUI 컴포넌트를 연결하세요.")]
    public TMP_Text objectCountText; // TextMeshPro UI 연결용
    private int remainingObjectCount = 0; // 남은 개수 추적용
    // 🌟 ▲ 유저 요청: 남은 오브젝트 수 표시용 UI 및 변수 ▲ 🌟

    // Inspector에서 할당할 생성용 프리팹
    [Header("Object Spawner")]
    public GameObject prefabToSpawn;
    [Tooltip("유니티 인스펙터에서 직접 생성할 위치를 지정합니다.")]
    public Vector3[] spawnPositionsFromInspector; // 인스펙터에서 직접 위치를 설정할 배열

    // 🌟 타겟 오브젝트의 비주얼 변경 설정
    [Header("Target Visuals (Crosshair/Focus Object)")]
    [Tooltip("게임 상태에 따라 비주얼이 변경될 대상 GameObject (SpriteRenderer 또는 Image 컴포넌트가 필요)")]
    public GameObject targetVisualObject;
    [Tooltip("Playing 상태가 아닐 때(기본값) 사용할 Sprite입니다.")]
    public Sprite defaultSprite;
    [Tooltip("클릭 가능한 오브젝트에 시선을 맞췄을 때 사용할 강조 Sprite입니다.")]
    public Sprite highlightedSprite;


    void Awake()
    {
        // 싱글턴 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않음
        }
        else
        {
            Destroy(gameObject); // 이미 인스턴스가 있으면 자신을 파괴
            return;
        }

        // 씬이 로드될 때마다 OnSceneLoaded 함수를 실행하도록 등록합니다.
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 초기 상태를 Starting으로 설정
        ChangeState(GameState.Starting);
    }

    // 씬이 성공적으로 로드되었을 때 호출되는 함수입니다.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameManager] 씬 로드 완료: {scene.name}");

        // 새 씬이 로드되면 항상 'Playing' 상태로 시작하고 관련 로직을 실행합니다.
        ChangeState(GameState.Playing);

        // 인스펙터에 설정된 위치에 오브젝트들을 생성합니다.
        if (spawnPositionsFromInspector != null && spawnPositionsFromInspector.Length > 0)
        {
            // SpawnObjects 함수 내부에 이미 Playing 상태인지 확인하는 로직이 있습니다.
            SpawnObjects(spawnPositionsFromInspector);
        }
        else
        {
            // 스폰 위치가 없을 경우 0으로 초기화 및 표시
            remainingObjectCount = 0;
            UpdateObjectCountUI();
        }
    }

    // GameManager 오브젝트가 파괴될 때(예: 게임 종료) 이벤트 구독을 해제합니다.
    void OnDestroy()
    {
        // SceneManager에 등록했던 함수를 해제해야 메모리 누수가 발생하지 않습니다.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 🌟 핵심 기능: N개의 GameObject를 지정된 위치 배열에 생성 🌟
    // 외부 스크립트에서 Vector3 배열을 매개변수로 넘겨주면 그 위치에 생성합니다.
    public void SpawnObjects(Vector3[] positions)
    {
        if (CurrentGameState != GameState.Playing)
        {
            Debug.LogWarning("[GameManager] 게임 플레이 중이 아닐 때는 오브젝트를 생성할 수 없습니다.");
            return;
        }

        if (prefabToSpawn == null)
        {
            Debug.LogError("생성할 프리팹(PrefabToSpawn)이 GameManager Inspector에 할당되지 않았습니다!");
            return;
        }

        // 🌟 [추가] 생성 전, 남은 개수를 생성할 위치의 개수만큼 설정
        remainingObjectCount = positions.Length;
        UpdateObjectCountUI(); // UI 즉시 갱신

        foreach (Vector3 pos in positions)
        {
            // 🛠️ 무작위 Y 회전값 계산 (0도에서 360도 사이)
            float randomYRotation = Random.Range(0f, 360f);

            // Quaternion.Euler(X, Y, Z)를 사용하여 무작위 Y 회전값을 갖는 Quaternion 생성
            Quaternion randomRotation = Quaternion.Euler(0, randomYRotation, 0);

            // 1. GameObject 생성
            GameObject newObject = Instantiate(prefabToSpawn, pos, randomRotation);
            newObject.name = "ClickableObject_" + pos.ToString();

            // 💡 [추가] ClickableObject 컴포넌트가 없다면 추가해 줍니다.
            if (newObject.GetComponent<ClickableObject>() == null)
            {
                newObject.AddComponent<ClickableObject>();
            }
        }
    }

    // 🌟 오브젝트 클릭 시 호출되는 함수 🌟
    public void OnObjectClicked(GameObject clickedObject)
    {
        if (CurrentGameState != GameState.Playing) return; // 플레이 중이 아니면 무시

        Debug.Log($"[GameManager] 오브젝트 클릭됨: {clickedObject.name} (점수 획득!)");

        // 1. 점수 획득 (GameManager의 역할)
        Score += 10;

        // 🌟 [추가] 남은 개수 차감 및 UI 업데이트
        remainingObjectCount--;
        if (remainingObjectCount < 0) remainingObjectCount = 0; // 안전장치
        UpdateObjectCountUI();

        // 2. 즉시 파괴 대신, 오브젝트에게 소리 재생 후 파괴를 요청합니다. (ClickableObject의 역할)
        if (clickedObject.TryGetComponent<ClickableObject>(out var clickable))
        {
            // 오브젝트에게 파괴 시퀀스를 시작하라고 지시합니다.
            clickable.StartDestructionSequence();
        }
        else
        {
            // ClickableObject 스크립트가 없다면, 기존처럼 즉시 파괴합니다.
            Destroy(clickedObject);
        }

        // (선택 사항) 모든 오브젝트를 다 찾았을 때 로직
        if (remainingObjectCount == 0)
        {
            Debug.Log("모든 오브젝트를 찾았습니다!");
            // 예: ChangeState(GameState.GameOver);
        }
    }

    // 🌟 [추가] UI 텍스트 업데이트 헬퍼 함수
    private void UpdateObjectCountUI()
    {
        if (objectCountText != null)
        {
            objectCountText.text = $"{remainingObjectCount}";
        }
        else
        {
            Debug.LogWarning("[GameManager] Object Count Text가 할당되지 않았습니다.");
        }
    }

    // --- ▼ 크로스헤어 비주얼 업데이트 함수 (FirstPersonController에서 호출) ▼ ---

    public void SetCrosshairVisuals(bool isTargetingClickable)
    {
        if (targetVisualObject == null || defaultSprite == null || highlightedSprite == null)
        {
            if (targetVisualObject == null) Debug.LogWarning("[GameManager] targetVisualObject가 할당되지 않았습니다. 크로스헤어 비주얼 업데이트 스킵.");
            return;
        }

        Sprite newSprite = isTargetingClickable ? highlightedSprite : defaultSprite;

        Image imageComponent = targetVisualObject.GetComponent<Image>();
        SpriteRenderer spriteRenderer = targetVisualObject.GetComponent<SpriteRenderer>();

        if (imageComponent != null)
        {
            if (imageComponent.sprite != newSprite) imageComponent.sprite = newSprite;
        }
        else if (spriteRenderer != null)
        {
            if (spriteRenderer.sprite != newSprite) spriteRenderer.sprite = newSprite;
        }
    }

    // --- ▼ 기존 함수들 (ChangeState) ---

    public void ChangeState(GameState newState)
    {
        if (CurrentGameState == newState) return;

        CurrentGameState = newState;
        Debug.Log("[GameManager] 상태 변경: " + newState);

        switch (newState)
        {
            case GameState.Starting:
                Time.timeScale = 1f;
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                Debug.Log("게임 오버! 최종 점수: " + score);
                break;
        }
    }

    public void TogglePause()
    {
        if (CurrentGameState == GameState.Playing) ChangeState(GameState.Paused);
        else if (CurrentGameState == GameState.Paused) ChangeState(GameState.Playing);
    }

    public void LoadScene(string sceneName)
    {
        Debug.Log("[GameManager] 씬 로드: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    public void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            nextSceneIndex = 0;
        }

        Debug.Log($"[GameManager] 다음 씬 로드 (Index: {nextSceneIndex})");
        SceneManager.LoadScene(nextSceneIndex);
    }

    public void RestartLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log("[GameManager] 현재 씬 재시작: " + currentSceneName);
        LoadScene(currentSceneName);
    }
}