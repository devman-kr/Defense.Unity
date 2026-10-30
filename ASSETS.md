# ASSETS — 외부 파일 장부

이 저장소에 든 외부 파일과 `Packages/manifest.json` 으로 참조하는 추가 패키지의 출처 · 라이선스를 적는다. **장부에 없는 외부 파일은 커밋하지 않는다.** AI 생성물은 「AI 생성(도구 · 모델)」으로 적는다. 유료 에셋은 쓰지 않고, Unity Asset Store 「무료」 에셋은 재배포 금지라 커밋하지 않는다.

## 파일

| 파일 | 출처 | 라이선스 | 수정 | AI 생성 | 들어온 날 |
|---|---|---|---|---|---|
| `.gitignore` | [github/gitignore `Unity.gitignore`](https://github.com/github/gitignore/blob/main/Unity.gitignore) 커밋 `301e18631acc` | CC0-1.0 | 끝에 `.DS_Store` · `.vscode/` 두 줄 | 아님 | 2026-10-30 |
| `Assets/Settings/`(빌드 프로필 제외) · `Assets/Scenes/SampleScene.unity` · `Assets/TutorialInfo/` · `Assets/Readme.asset` · `Assets/InputSystem_Actions.inputactions` | Unity 6000.3.27f1 프로젝트 템플릿 「Universal 3D」(`com.unity.template.urp-blank` 17.1.0)이 만든 파일 | [Unity Companion License](http://www.unity3d.com/legal/licenses/Unity_Companion_License)(템플릿 패키지 `LICENSE.md`) | `PC_RPAsset.asset` 은 에디터가 URP 17.3 형식으로 자동 변환 | 아님 | 2026-10-30 |

## 추가 패키지

템플릿 기본 패키지 밖에서 더한 것만 적는다. 파일은 저장소에 없고 Unity 가 레지스트리에서 받는다.

| 패키지 | 판 | 출처 | 라이선스 | 왜 | 들어온 날 |
|---|---|---|---|---|---|
| `com.unity.pipeline` | 0.9.1-exp.1(실험판) | Unity 레지스트리 | Unity Terms of Service(패키지 `LICENSE.md`) | Unity CLI 가 열린 에디터를 다룬다(`unity command`). 런타임 어셈블리는 `ENABLE_RUNTIME_PIPELINE` · `ENABLE_RUNTIME_PIPELINE_DEV` 를 정의할 때만 플레이어 빌드에 들어간다(asmdef `defineConstraints`) | 2026-10-30 |
