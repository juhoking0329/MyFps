# Project-Scoped Rules for MyFps

개발 및 코드 작성 시 준수해야 하는 규칙 리스트입니다:

1. **지역 분할 (#region, #endregion)**
   - 스크립트 작성 시 논리적 단위(예: Variables, Unity Event Methods, Custom Methods 등)별로 `#region`과 `#endregion`을 적극 활용하여 구역을 나눕니다.

2. **주석 작성 방식 (//)**
   - 코드를 쓰기 전에 해당 블록이 어떤 역할을 할지 `// 설명` 주석을 한글로 작성하거나, 코드 작성 후 상세히 주석을 명시합니다.

3. **변수 선언 및 인스펙터 노출**
   - Variables 파트에서 인스펙터 창을 통해 조절하면 유용한 변수들은 `[Header("그룹 이름")]`과 `[SerializeField]`를 사용하여 사용자가 다루기 쉽고 보기 편하게 설정합니다.

4. **유니티 이벤트 메서드 내 주석**
   - Awake, Start, Update, OnEnable 등 Unity Event Method 내에서도 각 연산 및 제어가 무엇을 수행하는지 명확한 주석을 작성합니다.

5. **메서드 분리 및 Custom Methods 사용**
   - 복잡한 로직은 유니티 생명주기 메서드(Update 등)에 직접 넣지 말고, `Custom Methods` 리전 내에 별도의 의미 있는 메서드(예: `CheckGround()`, `Move()` 등)를 정의하여 호출합니다.
