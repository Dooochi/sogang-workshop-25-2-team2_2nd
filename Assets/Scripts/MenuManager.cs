using UnityEngine;
using UnityEngine.UI; // UI 요소(Button)에 접근하기 위해 필요합니다.
using UnityEngine.EventSystems; // 이벤트 시스템 조작을 위해 필요합니다.
using UnityEngine.SceneManagement;
using System.Collections.Generic; // List를 사용하기 위해 필요합니다.

public class MenuManager : MonoBehaviour
{
    // Unity 에디터에서 드래그 앤 드롭으로 버튼들을 연결할 리스트입니다.
    public List<Button> menuButtons;

    // 현재 선택된 버튼의 인덱스입니다.
    private int selectedIndex = 0;

    // 키 입력 지연 시간을 위한 변수입니다.
    private float verticalInputTimer = 0f;
    private const float InputDelay = 0.2f; // 0.2초마다 한 번씩 입력 처리

    void Start()
    {
        // 메뉴 버튼이 하나라도 있는지 확인하고, 없으면 경고합니다.
        if (menuButtons.Count == 0)
        {
            Debug.LogError("MenuManager에 연결된 버튼이 없습니다!");
            return;
        }

        // 게임 시작 시 첫 번째 버튼을 자동으로 선택(Select)합니다.
        // EventSystem이 포커스를 받도록 합니다.
        SelectButton(0);
    }

    void Update()
    {
        // 마우스 클릭은 Button 컴포넌트가 자동으로 처리합니다. 
        // 여기서는 키보드 입력(WS, 화살표, 엔터)만 처리합니다.

        // 입력 타이머 업데이트
        verticalInputTimer += Time.deltaTime;

        // 수직 이동 입력 처리 (WS 또는 상하 화살표)
        float verticalInput = Input.GetAxisRaw("Vertical");

        if (verticalInputTimer >= InputDelay)
        {
            if (verticalInput > 0.1f) // 위로 이동 (W 또는 위 화살표)
            {
                Navigate(-1); // 인덱스를 감소
                verticalInputTimer = 0f;
            }
            else if (verticalInput < -0.1f) // 아래로 이동 (S 또는 아래 화살표)
            {
                Navigate(1); // 인덱스를 증가
                verticalInputTimer = 0f;
            }
        }

        // 메뉴 선택(엔터) 처리
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // 현재 선택된 버튼을 클릭(실행)합니다.
            ExecuteSelectedButton();
        }
    }

    // 메뉴를 위아래로 탐색하는 함수
    private void Navigate(int direction)
    {
        // 새로운 인덱스 계산
        selectedIndex += direction;

        // 인덱스가 범위를 벗어나지 않도록 Wrap Around (순환) 처리합니다.
        if (selectedIndex >= menuButtons.Count)
        {
            selectedIndex = 0; // 맨 아래에서 아래로 가면 맨 위로
        }
        else if (selectedIndex < 0)
        {
            selectedIndex = menuButtons.Count - 1; // 맨 위에서 위로 가면 맨 아래로
        }

        // 새로운 버튼을 선택(Select)하고 포커스를 옮깁니다.
        SelectButton(selectedIndex);
    }

    // 특정 인덱스의 버튼을 선택 상태로 만드는 함수
    private void SelectButton(int index)
    {
        // EventSystem이 해당 버튼에 포커스를 맞추도록 합니다.
        menuButtons[index].Select();
    }

    // 현재 선택된 버튼의 OnClick 이벤트를 실행하는 함수
    private void ExecuteSelectedButton()
    {
        // 버튼 컴포넌트가 가진 클릭 이벤트를 강제로 실행합니다.
        menuButtons[selectedIndex].onClick.Invoke();
    }

    public void StartGame()
    {
        Debug.Log("새 게임 시작!");
        // TODO: "GameScene"을 실제 게임 씬의 이름으로 변경해야 합니다.
        // 씬을 로드하기 전에 'File > Build Settings'에 해당 씬이 추가되어 있어야 합니다.
        SceneManager.LoadScene("MainScene");
    }

    public void EndGame()
    {
        Debug.Log("게임 종료!");
        // 유니티 에디터에서는 게임을 중지시킵니다.
        // 빌드된 게임(exe 등)에서는 애플리케이션을 종료합니다.
        Application.Quit();

#if UNITY_EDITOR
        // 에디터에서 테스트할 때만 필요합니다.
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}