using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Animations;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;

public class AutoWireProject : EditorWindow {
    
    public GameObject chadFbx;
    public GameObject policeBoxPrefab;
    public GameObject streetLightPrefab;
    public GameObject treePrefab;
    public GameObject carPrefab1;
    public GameObject carPrefab2;
    public GameObject roadPrefab;
    
    public Sprite retroWindowBase;
    public Sprite retroWindowButton;
    public Sprite retroWindowFill;
    public Sprite retroWindowHeader;
    public Sprite retroWindowSunken;
    public Sprite angryIcon;
    public Sprite flagIcon;
    
    public Font retroFont;
    public Texture2D grassTexture;
    public Texture2D groundTexture;
    public Texture2D skyTexture;
    public Texture2D playerTexture;

    [MenuItem("Game/Auto-Wire Project Config")]
    public static void ShowWindow() {
        GetWindow<AutoWireProject>("Auto-Wire Config");
    }

    void OnGUI() {
        GUILayout.Label("Asset Configuration", EditorStyles.boldLabel);
        
        chadFbx = (GameObject)EditorGUILayout.ObjectField("Kinetic Chad FBX", chadFbx, typeof(GameObject), false);
        policeBoxPrefab = (GameObject)EditorGUILayout.ObjectField("Police Box", policeBoxPrefab, typeof(GameObject), false);
        streetLightPrefab = (GameObject)EditorGUILayout.ObjectField("Street Light", streetLightPrefab, typeof(GameObject), false);
        treePrefab = (GameObject)EditorGUILayout.ObjectField("Tree", treePrefab, typeof(GameObject), false);
        carPrefab1 = (GameObject)EditorGUILayout.ObjectField("Car 1", carPrefab1, typeof(GameObject), false);
        carPrefab2 = (GameObject)EditorGUILayout.ObjectField("Car 2", carPrefab2, typeof(GameObject), false);
        roadPrefab = (GameObject)EditorGUILayout.ObjectField("Road", roadPrefab, typeof(GameObject), false);

        GUILayout.Space(10);
        GUILayout.Label("UI Sprites & Fonts", EditorStyles.boldLabel);
        retroWindowBase = (Sprite)EditorGUILayout.ObjectField("Window Base", retroWindowBase, typeof(Sprite), false);
        retroWindowButton = (Sprite)EditorGUILayout.ObjectField("Window Button", retroWindowButton, typeof(Sprite), false);
        retroWindowFill = (Sprite)EditorGUILayout.ObjectField("Window Fill", retroWindowFill, typeof(Sprite), false);
        retroWindowHeader = (Sprite)EditorGUILayout.ObjectField("Window Header", retroWindowHeader, typeof(Sprite), false);
        retroWindowSunken = (Sprite)EditorGUILayout.ObjectField("Window Sunken", retroWindowSunken, typeof(Sprite), false);
        angryIcon = (Sprite)EditorGUILayout.ObjectField("Angry Icon", angryIcon, typeof(Sprite), false);
        flagIcon = (Sprite)EditorGUILayout.ObjectField("Flag Icon", flagIcon, typeof(Sprite), false);
        retroFont = (Font)EditorGUILayout.ObjectField("Retro Font", retroFont, typeof(Font), false);

        GUILayout.Space(10);
        GUILayout.Label("Textures", EditorStyles.boldLabel);
        grassTexture = (Texture2D)EditorGUILayout.ObjectField("Grass Texture", grassTexture, typeof(Texture2D), false);
        groundTexture = (Texture2D)EditorGUILayout.ObjectField("Ground Texture", groundTexture, typeof(Texture2D), false);
        skyTexture = (Texture2D)EditorGUILayout.ObjectField("Sky Texture", skyTexture, typeof(Texture2D), false);
        playerTexture = (Texture2D)EditorGUILayout.ObjectField("Player Texture", playerTexture, typeof(Texture2D), false);

        GUILayout.Space(20);
        if (GUILayout.Button("Run Auto-Wire", GUILayout.Height(40))) {
            if (chadFbx == null || roadPrefab == null) {
                EditorUtility.DisplayDialog("Error", "Kinetic Chad FBX and Road Prefab are strictly required.", "OK");
                return;
            }
            WireProject();
        }
    }

    private void WireProject() {
        if (EditorApplication.isPlaying) {
            Debug.LogWarning("Please stop Play Mode before running Auto-Wire!");
            return;
        }

        InputSetup.SetupInputs();
        if (!Directory.Exists("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!Directory.Exists("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");

        // --- 1. PREFABS ---
        GameObject dropH = CreateDropPrefab("HealthDrop", Color.red, "Health");
        GameObject dropC = CreateDropPrefab("CoinDrop", Color.yellow, "Coin");
        GameObject dropP = CreateDropPrefab("PointsDrop", Color.blue, "Points");
        GameObject dropW = CreateDropPrefab("PowerDrop", Color.magenta, "Power");

        // --- ENEMY ANIMATOR ---
        string enemyCtrlPath = "Assets/Prefabs/EnemyAnim.controller";
        AnimatorController enemyCtrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(enemyCtrlPath);
        if (enemyCtrl == null) {
            enemyCtrl = AnimatorController.CreateAnimatorControllerAtPath(enemyCtrlPath);
            enemyCtrl.AddParameter("speed", AnimatorControllerParameterType.Float);
            enemyCtrl.AddParameter("attack", AnimatorControllerParameterType.Trigger);
            enemyCtrl.AddParameter("hit", AnimatorControllerParameterType.Trigger);
            enemyCtrl.AddParameter("death", AnimatorControllerParameterType.Trigger);

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(chadFbx));
            AnimatorStateMachine rootSm = enemyCtrl.layers[0].stateMachine;
            
            AnimatorState idle = rootSm.AddState("Idle");
            AnimatorState walk = rootSm.AddState("Walk");
            AnimatorState run = rootSm.AddState("Run");
            AnimatorState atk = rootSm.AddState("Attack");
            AnimatorState hit = rootSm.AddState("Hit");
            AnimatorState death = rootSm.AddState("Death");

            foreach (var asset in assets) {
                AnimationClip clip = asset as AnimationClip;
                if (clip == null || clip.name.Contains("__preview__")) continue;
                string lName = clip.name.ToLower();
                if (lName.Contains("idle")) idle.motion = clip;
                else if (lName == "walk") walk.motion = clip;
                else if (lName == "run") run.motion = clip;
                else if (lName.Contains("punch-hard")) atk.motion = clip;
                else if (lName.Contains("hit")) hit.motion = clip;
                else if (lName.Contains("death")) death.motion = clip;
            }
        }

        GameObject enemyObj = new GameObject("Enemy");
        GameObject meshE = PrefabUtility.InstantiatePrefab(chadFbx) as GameObject;
        meshE.transform.SetParent(enemyObj.transform);
        Animator eAnim = enemyObj.AddComponent<Animator>();
        eAnim.runtimeAnimatorController = enemyCtrl;
        enemyObj.layer = 2; // Enemy Layer
        CharacterController ec = enemyObj.AddComponent<CharacterController>();
        ec.height = 2f; ec.center = new Vector3(0, 1, 0);
        enemyObj.AddComponent<EnemyHealth>();
        enemyObj.AddComponent<EnemyFlocking>();
        enemyObj.AddComponent<EnemyAI>();
        GameObject enemyPrefab = SavePrefab(enemyObj, "Enemy");

        GameObject toughObj = new GameObject("EnemyTough");
        GameObject meshT = PrefabUtility.InstantiatePrefab(chadFbx) as GameObject;
        meshT.transform.SetParent(toughObj.transform);
        Animator tAnim = toughObj.AddComponent<Animator>();
        tAnim.runtimeAnimatorController = enemyCtrl;
        toughObj.layer = 2;
        toughObj.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        CharacterController tc = toughObj.AddComponent<CharacterController>();
        tc.height = 2f; tc.center = new Vector3(0, 1, 0);
        toughObj.AddComponent<EnemyHealth>().hp = 60;
        toughObj.AddComponent<EnemyFlocking>();
        toughObj.AddComponent<EnemyAI>();
        GameObject toughPrefab = SavePrefab(toughObj, "EnemyTough");

        GameObject bossObj = new GameObject("Boss");
        GameObject meshB = PrefabUtility.InstantiatePrefab(chadFbx) as GameObject;
        meshB.transform.SetParent(bossObj.transform);
        Animator bAnim = bossObj.AddComponent<Animator>();
        bAnim.runtimeAnimatorController = enemyCtrl;
        bossObj.layer = 2;
        bossObj.transform.localScale = new Vector3(3f, 3f, 3f);
        CharacterController bc = bossObj.AddComponent<CharacterController>();
        bc.height = 2f; bc.center = new Vector3(0, 1, 0);
        bossObj.AddComponent<BossAI>();
        bossObj.AddComponent<EnemyHealth>().hp = 300;
        GameObject bossPrefab = SavePrefab(bossObj, "Boss");

        // --- 3. ENV PROPS ---
        GameObject envGroup = new GameObject("EnvironmentProps");
        
        if (policeBoxPrefab != null) {
            GameObject pbox = PrefabUtility.InstantiatePrefab(policeBoxPrefab) as GameObject;
            pbox.transform.position = new Vector3(9.63f, 0.22f, -3.62f);
            pbox.transform.SetParent(envGroup.transform);
        }
        
        if (streetLightPrefab != null) {
            GameObject slight = PrefabUtility.InstantiatePrefab(streetLightPrefab) as GameObject;
            slight.transform.position = new Vector3(9.97f, 0.22f, 0.27f);
            slight.transform.SetParent(envGroup.transform);
        }
        
        if (treePrefab != null) {
            Vector3[] tPos = { new Vector3(6.57f, 0, 4.44f), new Vector3(1.02f, 0, 6.07f), new Vector3(-11.91f, 0, 8.19f), new Vector3(-5.46f, 0, -4.47f) };
            foreach(var tp in tPos) {
                GameObject t = PrefabUtility.InstantiatePrefab(treePrefab) as GameObject;
                t.transform.position = tp;
                t.transform.SetParent(envGroup.transform);
            }
        }
        
        if (carPrefab1 != null) {
            GameObject car = PrefabUtility.InstantiatePrefab(carPrefab1) as GameObject;
            car.transform.position = new Vector3(19.75f, 0, -0.87f);
            car.transform.SetParent(envGroup.transform);
        }
        
        if (carPrefab2 != null) {
            GameObject car2 = PrefabUtility.InstantiatePrefab(carPrefab2) as GameObject;
            car2.transform.position = new Vector3(-8.65f, 0, -20.22f);
            car2.transform.SetParent(envGroup.transform);
        }

        // --- 2. MAIN MENU SCENE ---
        var menuScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        Camera menuCam = new GameObject("Main Camera").AddComponent<Camera>();
        Color menuBg;
        ColorUtility.TryParseHtmlString("#A0A0A0", out menuBg);
        menuCam.backgroundColor = menuBg;
        menuCam.clearFlags = CameraClearFlags.SolidColor;

        CreateEventSystem();
        GameObject menuCanvas = CreateCanvas("MenuCanvas");
        
        GameObject sphere = new GameObject("WireframeSphere");
        sphere.AddComponent<MeshFilter>().mesh = GenerateSphere(13, 8);
        sphere.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Custom/RetroWireframe"));
        sphere.transform.position = new Vector3(-2.05f, -1.78f, 3.27f);
        sphere.transform.localScale = new Vector3(3, 3, 3);

        GameObject autoloads = GameObject.Find("Autoloads");
        if (autoloads == null) {
            autoloads = new GameObject("Autoloads");
            autoloads.AddComponent<SaveManager>();
        }

        MainMenu mm = menuCanvas.AddComponent<MainMenu>();
        mm.sphereBg = sphere.transform;
        
        GameObject taskbar = CreateUIPanel(menuCanvas, "Taskbar", Color.white);
        Image tbImg = taskbar.GetComponent<Image>();
        if (retroWindowBase != null) { tbImg.sprite = retroWindowBase; tbImg.type = Image.Type.Sliced; }
        RectTransform tbRt = taskbar.GetComponent<RectTransform>();
        tbRt.anchorMin = new Vector2(0, 0); tbRt.anchorMax = new Vector2(1, 0);
        tbRt.sizeDelta = new Vector2(0, 40); tbRt.anchoredPosition = new Vector2(0, 20);

        Button quitBtn = CreateButton(taskbar, "QuitButton", new Vector2(55, 0), "     QUIT", retroWindowButton);
        quitBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.5f);
        quitBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0.5f);
        quitBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 32);
        
        if (angryIcon != null) {
            Image flag = CreateImage(quitBtn.gameObject, "Icon", new Vector2(-30, 0), new Vector2(20, 20), Color.white);
            flag.sprite = angryIcon;
        }

        GameObject clockPnl = CreateUIPanel(taskbar, "ClockPanel", Color.white);
        Image clkImg = clockPnl.GetComponent<Image>();
        if (retroWindowSunken != null) { clkImg.sprite = retroWindowSunken; clkImg.type = Image.Type.Sliced; }
        RectTransform clkRt = clockPnl.GetComponent<RectTransform>();
        clkRt.anchorMin = new Vector2(1, 0.5f); clkRt.anchorMax = new Vector2(1, 0.5f);
        clkRt.sizeDelta = new Vector2(60, 30); clkRt.anchoredPosition = new Vector2(-35, 0);
        Text clockTxt = CreateText(clockPnl, "ClockText", Vector2.zero, "6:16");
        clockTxt.color = Color.black;

        GameObject winPnl = CreateUIPanel(menuCanvas, "GameWindow", Color.white);
        Image winImg = winPnl.GetComponent<Image>();
        if (retroWindowBase != null) { winImg.sprite = retroWindowBase; winImg.type = Image.Type.Sliced; }
        RectTransform winRt = winPnl.GetComponent<RectTransform>();
        winRt.anchorMin = new Vector2(0.6f, 0.3f); winRt.anchorMax = new Vector2(0.9f, 0.7f);
        winRt.sizeDelta = Vector2.zero; winRt.anchoredPosition = Vector2.zero;

        GameObject headerPnl = CreateUIPanel(winPnl, "Header", Color.white);
        Image hdrImg = headerPnl.GetComponent<Image>();
        if (retroWindowHeader != null) { hdrImg.sprite = retroWindowHeader; hdrImg.type = Image.Type.Sliced; }
        RectTransform hdrRt = headerPnl.GetComponent<RectTransform>();
        hdrRt.anchorMin = new Vector2(0, 1); hdrRt.anchorMax = new Vector2(1, 1);
        hdrRt.sizeDelta = new Vector2(-8, 24); hdrRt.anchoredPosition = new Vector2(0, -16);

        if (flagIcon != null) {
            GameObject flagObj = new GameObject("Flag", typeof(RectTransform));
            flagObj.transform.SetParent(headerPnl.transform, false);
            Image flagImg = flagObj.AddComponent<Image>();
            flagImg.sprite = flagIcon;
            RectTransform flagRt = flagObj.GetComponent<RectTransform>();
            flagRt.anchorMin = new Vector2(0, 0.5f); flagRt.anchorMax = new Vector2(0, 0.5f);
            flagRt.sizeDelta = new Vector2(16, 16); flagRt.anchoredPosition = new Vector2(16, 0);
        }

        Button xBtn = CreateButton(headerPnl, "CloseBtn", new Vector2(-16, 0), "X", retroWindowButton);
        xBtn.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0.5f);
        xBtn.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.5f);
        xBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 20);
        xBtn.GetComponentInChildren<Text>().color = Color.black;

        Text bgTitle = CreateText(menuCanvas, "BGTitle", new Vector2(-150, -50), "NON-NULL");
        bgTitle.color = Color.white;
        bgTitle.fontSize = 41;
        bgTitle.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        bgTitle.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);

        Button startBtn = CreateButton(winPnl, "StartButton", new Vector2(0, -9), "START GAME", retroWindowButton);
        
        UnityEditor.Events.UnityEventTools.AddPersistentListener(startBtn.onClick, mm.StartGame);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(quitBtn.onClick, mm.QuitGame);
        
        EditorSceneManager.SaveScene(menuScene, "Assets/Scenes/MainMenu.unity");

        // --- 3. LOADING SCENE ---
        var loadScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Camera loadCam = new GameObject("Main Camera").AddComponent<Camera>();
        Color loadBgColor;
        loadCam.backgroundColor = ColorUtility.TryParseHtmlString("#3A6EA5", out loadBgColor) ? loadBgColor : Color.blue;
        loadCam.clearFlags = CameraClearFlags.SolidColor;
        GameObject loadCanvas = CreateCanvas("LoadCanvas");
        CreateText(loadCanvas, "LoadText", new Vector2(0, 50), "Loading...");
        Image loadBar = CreateImage(loadCanvas, "LoadBar", new Vector2(0, -50), new Vector2(400, 40), Color.white);
        loadBar.type = Image.Type.Filled; loadBar.fillMethod = Image.FillMethod.Horizontal; loadBar.fillAmount = 0;
        LoadingScreen ls = new GameObject("LoadingManager").AddComponent<LoadingScreen>();
        ls.progressBar = loadBar;
        EditorSceneManager.SaveScene(loadScene, "Assets/Scenes/LoadingScreen.unity");

        // --- 4. LEVEL 1 SCENE ---
        var levelScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        CreateEventSystem();
        
        GameObject ground = (GameObject)PrefabUtility.InstantiatePrefab(roadPrefab);
        ground.name = "RoadTerrain";
        ground.transform.localScale = new Vector3(15f, 1f, 15f);
        if (ground.GetComponent<Collider>() == null) ground.AddComponent<BoxCollider>();
        
        if (grassTexture != null) {
            Material grassMat = new Material(Shader.Find("Standard"));
            grassMat.mainTexture = grassTexture;
            grassMat.mainTextureScale = new Vector2(5f, 5f); 
            foreach (var r in ground.GetComponentsInChildren<Renderer>()) {
                r.sharedMaterial = grassMat;
            }
        }
        
        CreateBoundary("WallN", new Vector3(0, 10, 500), new Vector3(1000, 20, 2));
        CreateBoundary("WallS", new Vector3(0, 10, -500), new Vector3(1000, 20, 2));
        CreateBoundary("WallE", new Vector3(500, 10, 0), new Vector3(2, 20, 1000));
        CreateBoundary("WallW", new Vector3(-500, 10, 0), new Vector3(2, 20, 1000));

        GameObject playerObj = (GameObject)PrefabUtility.InstantiatePrefab(chadFbx);
        playerObj.name = "Player";
        
        if (playerObj.GetComponent<Collider>() == null && playerObj.GetComponent<CharacterController>() == null) {
            CharacterController cc = playerObj.AddComponent<CharacterController>();
            cc.height = 2f; cc.center = new Vector3(0, 1, 0);
        }
        
        if (playerTexture != null) {
            Material unlitMat = new Material(Shader.Find("Unlit/Texture"));
            unlitMat.mainTexture = playerTexture;
            Renderer[] rs = playerObj.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rs) r.sharedMaterial = unlitMat;
        }

        Animator anim = playerObj.GetComponent<Animator>();
        if (anim != null) {
            string ctrlPath = "Assets/Prefabs/PlayerAnim.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
            controller.parameters = new AnimatorControllerParameter[0];
            AnimatorStateMachine rootSm = controller.layers[0].stateMachine;
            foreach (var state in rootSm.states) rootSm.RemoveState(state.state);

            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Kick", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("TornadoKick", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Punch", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("PunchHard", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Elbow", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);

            AnimatorState Pidle = rootSm.AddState("Idle");
            AnimatorState Pwalk = rootSm.AddState("Walk");
            AnimatorState Prun = rootSm.AddState("Run");
            AnimatorState Pkick = rootSm.AddState("Kick");
            AnimatorState Ptornado = rootSm.AddState("TornadoKick");
            AnimatorState Ppunch = rootSm.AddState("Punch");
            AnimatorState PpunchHard = rootSm.AddState("PunchHard");
            AnimatorState Pelbow = rootSm.AddState("Elbow");

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(chadFbx));
            foreach (var asset in assets) {
                AnimationClip clip = asset as AnimationClip;
                if (clip == null) continue;
                string n = clip.name.ToLower();
                if (n.Contains("idle")) Pidle.motion = clip;
                else if (n.Contains("walk")) Pwalk.motion = clip;
                else if (n.Contains("run")) Prun.motion = clip;
                else if (n.Contains("tornado")) Ptornado.motion = clip;
                else if (n.Contains("kick")) Pkick.motion = clip;
                else if (n.Contains("hard")) PpunchHard.motion = clip;
                else if (n.Contains("punch")) Ppunch.motion = clip;
                else if (n.Contains("elbow")) Pelbow.motion = clip;
            }

            rootSm.defaultState = Pidle;
            var iw = Pidle.AddTransition(Pwalk); iw.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed"); iw.hasExitTime = false;
            var wr = Pwalk.AddTransition(Prun); wr.AddCondition(AnimatorConditionMode.Greater, 0.5f, "Speed"); wr.hasExitTime = false;
            var rw = Prun.AddTransition(Pwalk); rw.AddCondition(AnimatorConditionMode.Less, 0.5f, "Speed"); rw.hasExitTime = false;
            var wi = Pwalk.AddTransition(Pidle); wi.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed"); wi.hasExitTime = false;

            WireAttack(rootSm, Pkick, "Kick", Pidle);
            WireAttack(rootSm, Ptornado, "TornadoKick", Pidle);
            WireAttack(rootSm, Ppunch, "Punch", Pidle);
            WireAttack(rootSm, PpunchHard, "PunchHard", Pidle);
            WireAttack(rootSm, Pelbow, "Elbow", Pidle);
            anim.runtimeAnimatorController = controller;
        }
        
        playerObj.tag = "Player"; playerObj.layer = 1;
        playerObj.transform.position = new Vector3(0, 1, 0);
        PlayerHealth pHealth = playerObj.AddComponent<PlayerHealth>();
        playerObj.AddComponent<PlayerController>();

        Camera mainCam = Camera.main;
        if (mainCam == null) {
            mainCam = new GameObject("Main Camera").AddComponent<Camera>();
            mainCam.tag = "MainCamera";
        }
        PlayerCamera pc = mainCam.gameObject.AddComponent<PlayerCamera>();
        pc.pivot = playerObj.transform;

        GameObject uiCanvas = CreateCanvas("UICanvas");

        // --- HP Panel (20, 20, size: 300x100) ---
        GameObject hpPanelObj = CreateUIPanel(uiCanvas, "HPPanel", new Color(0.925f, 0.914f, 0.847f, 1f));
        RectTransform hpRt = hpPanelObj.GetComponent<RectTransform>();
        hpRt.anchorMin = new Vector2(0f, 1f); hpRt.anchorMax = new Vector2(0f, 1f); hpRt.pivot = new Vector2(0f, 1f);
        hpRt.anchoredPosition = new Vector2(20, -20); hpRt.sizeDelta = new Vector2(300, 100);
        if (retroWindowBase != null) { hpPanelObj.GetComponent<Image>().sprite = retroWindowBase; hpPanelObj.GetComponent<Image>().type = Image.Type.Sliced; }

        Text hpLabel = CreateText(hpPanelObj, "HPLabel", new Vector2(10, -15), "HP:");
        RectTransform hpLabelRt = hpLabel.GetComponent<RectTransform>();
        hpLabelRt.anchorMin = new Vector2(0, 1); hpLabelRt.anchorMax = new Vector2(0, 1); hpLabelRt.pivot = new Vector2(0, 1);
        hpLabelRt.sizeDelta = new Vector2(50, 30);
        hpLabel.color = Color.black; hpLabel.alignment = TextAnchor.MiddleLeft;

        Image lifeBar = CreateImage(hpPanelObj, "HPBar", new Vector2(70, -15), new Vector2(200, 30), Color.red);
        RectTransform lifeBarRt = lifeBar.GetComponent<RectTransform>();
        lifeBarRt.anchorMin = new Vector2(0, 1); lifeBarRt.anchorMax = new Vector2(0, 1); lifeBarRt.pivot = new Vector2(0, 1);
        lifeBar.type = Image.Type.Filled; lifeBar.fillMethod = Image.FillMethod.Horizontal;
        if (retroWindowFill != null) lifeBar.sprite = retroWindowFill;
        pHealth.lifeBar = lifeBar;

        Text multLabel = CreateText(hpPanelObj, "DamageMultiplierLabel", new Vector2(10, -60), "x2 DMG:");
        RectTransform multLabelRt = multLabel.GetComponent<RectTransform>();
        multLabelRt.anchorMin = new Vector2(0, 1); multLabelRt.anchorMax = new Vector2(0, 1); multLabelRt.pivot = new Vector2(0, 1);
        multLabelRt.sizeDelta = new Vector2(90, 30);
        multLabel.color = Color.black; multLabel.alignment = TextAnchor.MiddleLeft;

        Image dmgBar = CreateImage(hpPanelObj, "DamageMultiplierBar", new Vector2(110, -65), new Vector2(160, 20), Color.yellow);
        RectTransform dmgBarRt = dmgBar.GetComponent<RectTransform>();
        dmgBarRt.anchorMin = new Vector2(0, 1); dmgBarRt.anchorMax = new Vector2(0, 1); dmgBarRt.pivot = new Vector2(0, 1);
        dmgBar.type = Image.Type.Filled; dmgBar.fillMethod = Image.FillMethod.Horizontal;

        // --- Round Panel (640, 20, size: 300x140) ---
        GameObject roundPanelObj = CreateUIPanel(uiCanvas, "RoundPanel", new Color(0.925f, 0.914f, 0.847f, 1f));
        RectTransform rndRt = roundPanelObj.GetComponent<RectTransform>();
        rndRt.anchorMin = new Vector2(0f, 1f); rndRt.anchorMax = new Vector2(0f, 1f); rndRt.pivot = new Vector2(0f, 1f);
        rndRt.anchoredPosition = new Vector2(640, -20); rndRt.sizeDelta = new Vector2(300, 140);
        if (retroWindowBase != null) { roundPanelObj.GetComponent<Image>().sprite = retroWindowBase; roundPanelObj.GetComponent<Image>().type = Image.Type.Sliced; }

        Text rndText = CreateText(roundPanelObj, "RoundLabel", new Vector2(10, -10), "Round 1");
        RectTransform rndTextRt = rndText.GetComponent<RectTransform>();
        rndTextRt.anchorMin = new Vector2(0, 1); rndTextRt.anchorMax = new Vector2(0, 1); rndTextRt.pivot = new Vector2(0, 1);
        rndTextRt.sizeDelta = new Vector2(280, 25);
        rndText.color = Color.black; rndText.alignment = TextAnchor.MiddleLeft;

        Image roundBar = CreateImage(roundPanelObj, "RoundProgressBar", new Vector2(10, -40), new Vector2(260, 20), Color.green);
        RectTransform roundBarRt = roundBar.GetComponent<RectTransform>();
        roundBarRt.anchorMin = new Vector2(0, 1); roundBarRt.anchorMax = new Vector2(0, 1); roundBarRt.pivot = new Vector2(0, 1);
        roundBar.type = Image.Type.Filled; roundBar.fillMethod = Image.FillMethod.Horizontal;

        Text ptsText = CreateText(roundPanelObj, "PointsLabel", new Vector2(10, -65), "Points: 0");
        RectTransform ptsTextRt = ptsText.GetComponent<RectTransform>();
        ptsTextRt.anchorMin = new Vector2(0, 1); ptsTextRt.anchorMax = new Vector2(0, 1); ptsTextRt.pivot = new Vector2(0, 1);
        ptsTextRt.sizeDelta = new Vector2(280, 25);
        ptsText.color = Color.black; ptsText.alignment = TextAnchor.MiddleLeft;

        Text coinText = CreateText(roundPanelObj, "CoinsLabel", new Vector2(10, -95), "Coins: 0");
        RectTransform coinTextRt = coinText.GetComponent<RectTransform>();
        coinTextRt.anchorMin = new Vector2(0, 1); coinTextRt.anchorMax = new Vector2(0, 1); coinTextRt.pivot = new Vector2(0, 1);
        coinTextRt.sizeDelta = new Vector2(280, 25);
        coinText.color = Color.black; coinText.alignment = TextAnchor.MiddleLeft;

        // --- Round Banner (0, 200, size: 960x100) ---
        GameObject bannerObj = CreateUIPanel(uiCanvas, "RoundBanner", new Color(0, 0, 0, 0.4f));
        RectTransform bRt = bannerObj.GetComponent<RectTransform>();
        bRt.anchorMin = new Vector2(0.5f, 0.5f); bRt.anchorMax = new Vector2(0.5f, 0.5f); bRt.pivot = new Vector2(0.5f, 0.5f);
        bRt.anchoredPosition = new Vector2(0, -72); bRt.sizeDelta = new Vector2(960, 100);
        Text bannerText = CreateText(bannerObj, "RoundBannerText", Vector2.zero, "ROUND 1 - FIGHT!");
        RectTransform bannerTextRt = bannerText.GetComponent<RectTransform>();
        bannerTextRt.anchorMin = Vector2.zero; bannerTextRt.anchorMax = Vector2.one; bannerTextRt.sizeDelta = Vector2.zero;
        bannerText.color = Color.white; bannerText.alignment = TextAnchor.MiddleCenter;

        // --- XPUI Component on UICanvas ---
        XPUI xpui = uiCanvas.AddComponent<XPUI>();
        xpui.hpPanel = hpRt;
        xpui.hpLabel = hpLabel;
        xpui.hpBar = lifeBar;
        xpui.damageMultiplierLabel = multLabel;
        xpui.damageMultiplierBar = dmgBar;
        xpui.roundPanel = rndRt;
        xpui.roundLabel = rndText;
        xpui.roundProgressBar = roundBar;
        xpui.pointsLabel = ptsText;
        xpui.coinsLabel = coinText;
        xpui.roundBanner = bRt;
        xpui.roundBannerText = bannerText;

        GameObject goPanel = CreateUIPanel(uiCanvas, "GameOverScreen", new Color(0,0,0,0.8f));
        RectTransform goRt = goPanel.GetComponent<RectTransform>();
        goRt.anchorMin = new Vector2(0.5f, 0.5f); goRt.anchorMax = new Vector2(0.5f, 0.5f);
        goRt.sizeDelta = new Vector2(400, 300); goRt.anchoredPosition = Vector2.zero;
        if (retroWindowBase != null) { goPanel.GetComponent<Image>().sprite = retroWindowBase; goPanel.GetComponent<Image>().color = Color.white; goPanel.GetComponent<Image>().type = Image.Type.Sliced; }
        CanvasGroup goGroup = goPanel.AddComponent<CanvasGroup>();
        goGroup.alpha = 0; goGroup.interactable = false; goGroup.blocksRaycasts = false;
        CreateText(goPanel, "GOTitle", new Vector2(0, 100), "GAME OVER").color = Color.red;
        Button goRetryBtn = CreateButton(goPanel, "RetryButton", new Vector2(0, 0), "RETRY", retroWindowButton);
        Button goMenuBtn = CreateButton(goPanel, "MenuButton", new Vector2(0, -60), "MAIN MENU", retroWindowButton);

        GameObject pPanel = CreateUIPanel(uiCanvas, "PauseMenu", new Color(0,0,0,0.5f));
        RectTransform pRt = pPanel.GetComponent<RectTransform>();
        pRt.anchorMin = new Vector2(0.5f, 0.5f); pRt.anchorMax = new Vector2(0.5f, 0.5f);
        pRt.sizeDelta = new Vector2(400, 300); pRt.anchoredPosition = Vector2.zero;
        if (retroWindowBase != null) { pPanel.GetComponent<Image>().sprite = retroWindowBase; pPanel.GetComponent<Image>().color = Color.white; pPanel.GetComponent<Image>().type = Image.Type.Sliced; }
        CanvasGroup pGroup = pPanel.AddComponent<CanvasGroup>();
        pGroup.alpha = 0; pGroup.interactable = false; pGroup.blocksRaycasts = false;
        CreateText(pPanel, "PauseTitle", new Vector2(0, 100), "PAUSED").color = Color.black;
        Button pResumeBtn = CreateButton(pPanel, "ResumeButton", new Vector2(0, 0), "RESUME", retroWindowButton);
        Button pMenuBtn = CreateButton(pPanel, "MenuButton", new Vector2(0, -60), "MAIN MENU", retroWindowButton);
        
        PauseMenu pmScript = uiCanvas.AddComponent<PauseMenu>();
        pmScript.pauseGroup = pGroup;
        pmScript.resumeBtn = pResumeBtn;
        pmScript.mainMenuBtn = pMenuBtn;

        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();
        gm.gameOverGroup = goGroup;
        gm.retryBtn = goRetryBtn;
        gm.mainMenuBtn = goMenuBtn;
        gm.pointsText = ptsText;
        gm.coinsText = coinText;
        gm.damageBar = dmgBar;
        
        GameObject fmObj = new GameObject("FlockingManager");
        FlockingManager fm = fmObj.AddComponent<FlockingManager>();
        fm.player = playerObj.transform;

        GameObject dmObj = new GameObject("DropManager");
        DropManager dm = dmObj.AddComponent<DropManager>();
        
        GameObject smObj = new GameObject("EnemySpawner");
        EnemySpawner sm = smObj.AddComponent<EnemySpawner>();
        sm.bossPrefab = bossPrefab;
        sm.player = playerObj.transform;
        sm.roundText = rndText;
        sm.roundProgressBar = roundBar;

        GameObject pmObj = new GameObject("PoolManager");
        PoolManager pm = pmObj.AddComponent<PoolManager>();
        pm.weakEnemyPrefab = enemyPrefab;
        pm.toughEnemyPrefab = toughPrefab;
        pm.coinPrefab = dropC;
        pm.healthPrefab = dropH;
        pm.pointsPrefab = dropP;
        pm.powerupPrefab = dropW;

        GameObject apObj = new GameObject("AudioPool");
        AudioPool ap = apObj.AddComponent<AudioPool>();

        if (skyTexture != null) {
            Material skyMat = new Material(Shader.Find("Skybox/Panoramic"));
            skyMat.mainTexture = skyTexture;
            RenderSettings.skybox = skyMat;
        }
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.443f, 0.443f, 0.443f, 1f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 60f;
        RenderSettings.fogEndDistance = 150f;

        EditorSceneManager.SaveScene(levelScene, "Assets/Scenes/Level1.unity");
        
        // --- 5. BUILD SETTINGS FIX ---
        EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[] {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/LoadingScreen.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Level1.unity", true)
        };
        EditorBuildSettings.scenes = buildScenes;
        
        Debug.Log("Auto-Wire Complete! Scenes generated and added to Build Settings.");
    }

    private GameObject CreateCanvas(string name) {
        GameObject canvasGO = new GameObject(name, typeof(RectTransform));
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(960, 544);
        canvasGO.AddComponent<GraphicRaycaster>();
        return canvasGO;
    }

    private GameObject CreateUIPanel(GameObject parent, string name, Color bgColor) {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent.transform, false);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        
        Image img = panel.AddComponent<Image>();
        img.color = bgColor;
        img.raycastTarget = false;
        return panel;
    }

    private Image CreateImage(GameObject parent, string name, Vector2 pos, Vector2 size, Color c) {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent.transform, false);
        Image img = obj.AddComponent<Image>();
        img.color = c;
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        if (pos.x > 0 && pos.y < 0) { rt.anchorMin=new Vector2(0,1); rt.anchorMax=new Vector2(0,1); rt.pivot=new Vector2(0,1); }
        else if (pos.x < 0 && pos.y < 0) { rt.anchorMin=new Vector2(1,1); rt.anchorMax=new Vector2(1,1); rt.pivot=new Vector2(1,1); }
        return img;
    }

    private Text CreateText(GameObject parent, string name, Vector2 pos, string txt) {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent.transform, false);
        Text text = obj.AddComponent<Text>();
        text.text = txt;
        text.font = retroFont != null ? retroFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.color = Color.white;
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(300, 50);
        if (pos.x < 0) { rt.anchorMin=new Vector2(1,1); rt.anchorMax=new Vector2(1,1); rt.pivot=new Vector2(1,1); text.alignment = TextAnchor.MiddleRight; }
        else { text.alignment = TextAnchor.MiddleCenter; }
        return text;
    }

    private Button CreateButton(GameObject parent, string name, Vector2 pos, string txt, Sprite sprite = null) {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent.transform, false);
        Image img = obj.AddComponent<Image>(); img.color = Color.white;
        if (sprite != null) {
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
        }
        Button btn = obj.AddComponent<Button>();
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(160, 40);
        Text t = CreateText(obj, "Text", Vector2.zero, txt);
        t.color = Color.black; t.alignment = TextAnchor.MiddleCenter;
        return btn;
    }

    private void CreateBoundary(string name, Vector3 pos, Vector3 scale) {
        GameObject wall = new GameObject(name);
        wall.layer = 3;
        wall.transform.position = pos;
        BoxCollider col = wall.AddComponent<BoxCollider>();
        col.size = scale;
    }

    private GameObject SavePrefab(GameObject obj, string name) {
        Object emptyPrefab = PrefabUtility.CreateEmptyPrefab("Assets/Prefabs/" + name + ".prefab");
        GameObject prefab = PrefabUtility.ReplacePrefab(obj, emptyPrefab, ReplacePrefabOptions.ConnectToPrefab);
        DestroyImmediate(obj);
        return prefab;
    }

    private GameObject CreateDropPrefab(string name, Color color, string type) {
        GameObject dropObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dropObj.name = name; dropObj.layer = 5;
        dropObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        dropObj.GetComponent<Collider>().isTrigger = true;
        Pickup pickup = dropObj.AddComponent<Pickup>();
        pickup.pickupType = type;
        return SavePrefab(dropObj, name);
    }

    private Mesh GenerateSphere(int segments, int rings) {
        Mesh mesh = new Mesh();
        int numVertices = (segments + 1) * (rings + 1);
        Vector3[] vertices = new Vector3[numVertices];
        Vector2[] uvs = new Vector2[numVertices];
        int[] triangles = new int[segments * rings * 6];

        int v = 0;
        for (int y = 0; y <= rings; y++) {
            float vFac = (float)y / rings;
            float lat = Mathf.PI * vFac - Mathf.PI / 2f;
            float yPos = Mathf.Sin(lat);
            float r = Mathf.Cos(lat);

            for (int x = 0; x <= segments; x++) {
                float uFac = (float)x / segments;
                float lon = uFac * Mathf.PI * 2f;
                vertices[v] = new Vector3(Mathf.Cos(lon) * r, yPos, Mathf.Sin(lon) * r);
                uvs[v] = new Vector2(uFac, vFac);
                v++;
            }
        }

        int t = 0;
        for (int y = 0; y < rings; y++) {
            for (int x = 0; x < segments; x++) {
                int curr = y * (segments + 1) + x;
                int next = curr + (segments + 1);

                triangles[t++] = curr;
                triangles[t++] = next;
                triangles[t++] = curr + 1;

                triangles[t++] = curr + 1;
                triangles[t++] = next;
                triangles[t++] = next + 1;
            }
        }
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    static void AddParamIfMissing(AnimatorController ctrl, string name, AnimatorControllerParameterType type) {
        foreach (var p in ctrl.parameters) {
            if (p.name == name) return;
        }
        ctrl.AddParameter(name, type);
    }

    static void WireAttack(AnimatorStateMachine sm, AnimatorState attack, string trigger, AnimatorState idle) {
        var t1 = sm.AddAnyStateTransition(attack);
        t1.AddCondition(AnimatorConditionMode.If, 0, trigger);
        t1.hasExitTime = false;
        t1.canTransitionToSelf = false;
        var t2 = attack.AddTransition(idle);
        t2.hasExitTime = true;
        t2.exitTime = 0.9f;
    }

    private void CreateEventSystem() {
        if (FindObjectOfType<EventSystem>() == null) {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }
    }
}
