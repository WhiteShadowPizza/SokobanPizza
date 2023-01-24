using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Sokoban : MonoBehaviour
{
    // タイルの種類
    private enum TileType
    {
        NONE, // 何も無い
        GROUND, // 地面
        TARGET, // 目的地
        PLAYER, // プレイヤー
        BLOCK, // ブロック

        PLAYER_ON_TARGET, // プレイヤー（目的地の上）
        BLOCK_ON_TARGET, // ブロック（目的地の上）
    }

    // 方向の種類
    private enum DirectionType
    {
        UP, // 上
        RIGHT, // 右
        DOWN, // 下
        LEFT, // 左
    }

    public TextAsset stageFile; // ステージ構造が記述されたテキストファイル

    private int rows = default; // 行数
    private int columns = default; // 列数
    private TileType[,] tileList = default; // タイル情報を管理する二次元配列

    public float tileSize; // タイルのサイズ

    [SerializeField] private Sprite groundSprite = default; // 地面のスプライト
    [SerializeField] private Sprite targetSprite = default; // 目的地のスプライト
    [SerializeField] private Sprite playerSprite = default; // プレイヤーのスプライト
    [SerializeField] private Sprite blockSprite = default; // ブロックのスプライト
    private SpriteRenderer pSprite;

    [SerializeField] private Sprite playerSpriteUp = default;
    [SerializeField] private Sprite playerSpriteDown = default;
    [SerializeField] private Sprite playerSpriteRight = default;
    [SerializeField] private Sprite playerSpriteLeft = default;

    private GameObject player; // プレイヤーのゲームオブジェクト
    private Vector2 middleOffset; // 中心位置
    private int blockCount; // ブロックの数
    private bool isClear; // ゲームをクリアした場合 true

    public int counter = 0; //歩数のカウンター
    public GameObject score_object = null; // Textオブジェクト
    public int score_num = 0; // スコア変数
    [SerializeField] private int maxCount = 0;
    [SerializeField] Text cleartext = default; //クリア時の文字
    [SerializeField] Text gameovertext = default;　//ゲームオーバー時の文字
    [SerializeField] bool playermove = true; //プレイヤーが動けるかの判断用


    // 各位置に存在するゲームオブジェクトを管理する連想配列
    private Dictionary<GameObject, Vector2Int> gameObjectPosTable = new Dictionary<GameObject, Vector2Int>();

    // ゲーム開始時に呼び出される
    private void Start()
    {
        LoadTileData(); // タイルの情報を読み込む
        CreateStage(); // ステージを作成
        player = GameObject.Find("player");
        pSprite = player.GetComponent<SpriteRenderer>();
        cleartext.enabled = false;
        gameovertext.enabled = false;
    }

    // タイルの情報を読み込む
    private void LoadTileData()
    {
        // タイルの情報を一行ごとに分割
        string[] lines = stageFile.text.Split
        (
            new[] { '\r', '\n' },
            StringSplitOptions.RemoveEmptyEntries
        );

        // タイルの列数を計算
        string[] nums = lines[0].Split(new[] { ',' });

        // タイルの列数と行数を保持
        rows = lines.Length; // 行数
        columns = nums.Length; // 列数

        // タイル情報を int 型の２次元配列で保持
        tileList = new TileType[columns, rows];
        for (int y = 0; y < rows; y++)
        {
            // 一文字ずつ取得
            string st = lines[y];
            nums = st.Split(new[] { ',' });
            for (int x = 0; x < columns; x++)
            {
                // 読み込んだ文字を数値に変換して保持
                tileList[x, y] = (TileType)int.Parse(nums[x]);
            }
        }
    }

    // ステージを作成
    private void CreateStage()
    {
        // ステージの中心位置を計算
        middleOffset.x = (columns * tileSize - tileSize) * 0.5f;
        middleOffset.y = (rows * tileSize - tileSize) * 0.5f;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                TileType val = tileList[x, y];

                // 何も無い場所は無視
                if (val == TileType.NONE)
                {
                    continue;
                }
                // タイルの名前に行番号と列番号を付与
                string name = "tile" + y + "_" + x;

                // タイルのゲームオブジェクトを作成
                GameObject tile = new GameObject(name);

                // タイルにスプライトを描画する機能を追加
                SpriteRenderer spriteRenderer = tile.AddComponent<SpriteRenderer>();

                // タイルのスプライトを設定
                spriteRenderer.sprite = groundSprite;

                // タイルの位置を設定
                tile.transform.position = GetDisplayPosition(x, y);

                switch (val)
                {
                    //目的地の場合
                    case TileType.TARGET:

                     //目的地のゲームオブジェクトを作成
                     GameObject destination = new GameObject("destination");
                 
                     // 目的地にスプライトを描画する機能を追加
                     spriteRenderer = destination.AddComponent<SpriteRenderer>();
                 
                     // 目的地のスプライトを設定
                     spriteRenderer.sprite = targetSprite;
                 
                     // 目的地の描画順を手前にする
                     spriteRenderer.sortingOrder = 1;
                 
                     // 目的地の位置を設定
                     destination.transform.position = GetDisplayPosition(x, y);

                     break;

                    //プレイヤーの場合
                    case TileType.PLAYER:

                     // プレイヤーのゲームオブジェクトを作成
                     player = new GameObject("player");
                 
                     // プレイヤーにスプライトを描画する機能を追加
                     spriteRenderer = player.AddComponent<SpriteRenderer>();
                 
                     // プレイヤーのスプライトを設定
                     spriteRenderer.sprite = playerSprite;
                 
                     // プレイヤーの描画順を手前にする
                     spriteRenderer.sortingOrder = 2;
                 
                     // プレイヤーの位置を設定
                     player.transform.position = GetDisplayPosition(x, y);
                 
                     // プレイヤーを連想配列に追加
                     gameObjectPosTable.Add(player, new Vector2Int(x, y));

                     break;

                    //ブロックの場合
                    case TileType.BLOCK:

                     //ブロックの数を増やす
                     blockCount++;
                 
                     // ブロックのゲームオブジェクトを作成
                     GameObject block = new GameObject("block" + blockCount);
                 
                     // ブロックにスプライトを描画する機能を追加
                     spriteRenderer = block.AddComponent<SpriteRenderer>();
                 
                     // ブロックのスプライトを設定
                     spriteRenderer.sprite = blockSprite;
                 
                     // ブロックの描画順を手前にする
                     spriteRenderer.sortingOrder = 2;
                 
                     // ブロックの位置を設定
                     block.transform.position = GetDisplayPosition(x, y);
                 
                     // ブロックを連想配列に追加
                     gameObjectPosTable.Add(block, new Vector2Int(x, y));

                     break;
                }
            }
        }
    }


    // 指定された行番号と列番号からスプライトの表示位置を計算して返す
    private Vector2 GetDisplayPosition(int x, int y)
    {
        return new Vector2
        (
            x * tileSize - middleOffset.x,
            y * -tileSize + middleOffset.y
        );
    }

    // 指定された位置に存在するゲームオブジェクトを返します
    private GameObject GetGameObjectAtPosition(Vector2Int pos)
    {
        foreach (KeyValuePair<GameObject, Vector2Int> pair in gameObjectPosTable)
        {
            // 指定された位置が見つかった場合
            if (pair.Value == pos)
            {
                // その位置に存在するゲームオブジェクトを返す
                return pair.Key;
            }
        }
        return null;
    }

    // 指定された位置のタイルがブロックなら true を返す
    private bool GetBlock(Vector2Int pos)
    {
        TileType cell = tileList[pos.x, pos.y];
        return cell == TileType.BLOCK || cell == TileType.BLOCK_ON_TARGET;
    }

    // 指定された位置がステージ内なら true を返す
    private bool GetValidPosition(Vector2Int pos)
    {
        if (0 <= pos.x && pos.x < columns && 0 <= pos.y && pos.y < rows)
        {
            return tileList[pos.x, pos.y] != TileType.NONE;
        }
        return false;
    }

    // 毎フレーム呼び出される
    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            // 現在のシーンを再読み込み
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // オブジェクトからTextコンポーネントを取得
        Text score_text = score_object.GetComponent<Text>();
        // テキストの表示を入れ替える
        score_text.text = "歩数:" + counter;

        // ゲームクリアしている場合は操作できないようにする
        if (isClear)
        {
            cleartext.enabled = true;
            return;
        }
        if (playermove == true)
        {
            // 上矢印が押された場合
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                // プレイヤーが上に移動できるか検証
                TryMovePlayer(DirectionType.UP);
            }
            // 右矢印が押された場合
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                // プレイヤーが右に移動できるか検証
                TryMovePlayer(DirectionType.RIGHT);
            }
            // 下矢印が押された場合
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                // プレイヤーが下に移動できるか検証
                TryMovePlayer(DirectionType.DOWN);
            }
            // 左矢印が押された場合
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                // プレイヤーが左に移動できるか検証
                TryMovePlayer(DirectionType.LEFT);
            }

            //歩数が限界歩数に到達した時
            if (counter >= maxCount)
            {
                //プレイヤーを動けなくしてゲームオーバーを表示
                playermove = false;
                gameovertext.enabled = true;
            }

            //歩数が限界歩数に到達した時とクリアが同時に発生した場合
            if (counter >= maxCount && isClear)
            {
                //プレイヤーを動けなくしてクリアを表示
                playermove = false;
                gameovertext.enabled = false;
                cleartext.enabled = true;
            }
        }
    }

    private void PlayerTile(Vector2Int current, Vector2Int next)
    {
        // プレイヤーの現在地のタイルの情報を更新
        UpdateGameObjectPosition(current);

        // プレイヤーを移動
        player.transform.position = GetDisplayPosition(next.x, next.y);

        // プレイヤーの位置を更新
        gameObjectPosTable[player] = next;

        // プレイヤーの移動先の番号を更新
        if (tileList[next.x, next.y] == TileType.GROUND)
        {
            // 移動先が地面ならプレイヤーの番号に更新
            tileList[next.x, next.y] = TileType.PLAYER;
        }
        else if (tileList[next.x, next.y] == TileType.TARGET)
        {
            // 移動先が目的地ならプレイヤー（目的地の上）の番号に更新
            tileList[next.x, next.y] = TileType.PLAYER_ON_TARGET;
        }
    }

    // 指定された方向にプレイヤーが移動できるか検証
    // 移動できる場合は移動する
    private void TryMovePlayer(DirectionType direction)
    {
        // プレイヤーの現在地を取得
        Vector2Int currentPlayerPos = gameObjectPosTable[player];

        // プレイヤーの移動先の位置を計算
        Vector2Int nextPlayerPos = GetNextPositionAlong(currentPlayerPos, direction);


        // プレイヤーの移動先がステージ内ではない場合は無視
        if (!GetValidPosition(nextPlayerPos))
        {
            return;
        }
           

        // プレイヤーの移動先にブロックが存在する場合
        if (GetBlock(nextPlayerPos))
        {                                                    
            // ブロックの移動先の位置を計算
            Vector2Int nextBlockPos = GetNextPositionAlong(nextPlayerPos, direction);

            // ブロックの移動先がステージ内の場合かつ
            // ブロックの移動先にブロックが存在しない場合
            if (GetValidPosition(nextBlockPos) && !GetBlock(nextBlockPos))
            {
                // 移動するブロックを取得
                GameObject block = GetGameObjectAtPosition(nextPlayerPos);

                // プレイヤーの移動先のタイルの情報を更新
                UpdateGameObjectPosition(nextPlayerPos);
                
                // ブロックを移動
                block.transform.position = GetDisplayPosition(nextBlockPos.x, nextBlockPos.y);
                
                // ブロックの位置を更新
                gameObjectPosTable[block] = nextBlockPos;
                
                // ブロックの移動先の番号を更新
                if (tileList[nextBlockPos.x, nextBlockPos.y] == TileType.GROUND)
                {
                    // 移動先が地面ならブロックの番号に更新
                    tileList[nextBlockPos.x, nextBlockPos.y] = TileType.BLOCK;
                    counter++;
                }
                else if (tileList[nextBlockPos.x, nextBlockPos.y] == TileType.TARGET)
                {
                    // 移動先が目的地ならブロック（目的地の上）の番号に更新
                    tileList[nextBlockPos.x, nextBlockPos.y] = TileType.BLOCK_ON_TARGET;
                    counter++;
                }

                PlayerTile(currentPlayerPos, nextPlayerPos);

            }
        }
        // プレイヤーの移動先にブロックが存在しない場合
        else
        {
            PlayerTile(currentPlayerPos, nextPlayerPos);
            counter++;
        }

        // ゲームをクリアしたかどうか確認
        CheckCompletion();
    }

    // 指定された方向の位置を返す
    private Vector2Int GetNextPositionAlong(Vector2Int pos, DirectionType direction)
    {
        switch (direction)
        {
            // 上
            case DirectionType.UP:
                pSprite.sprite = playerSpriteUp;
                pos.y -= 1;
                break;

            // 右
            case DirectionType.RIGHT:
                pSprite.sprite = playerSpriteRight;
                pos.x += 1;
                break;

            // 下
            case DirectionType.DOWN:
                pSprite.sprite = playerSpriteDown;
                pos.y += 1;
                break;

            // 左
            case DirectionType.LEFT:
                pSprite.sprite = playerSpriteLeft;
                pos.x -= 1;
                break;
        }
        return pos;
    }

    // 指定された位置のタイルを更新
    private void UpdateGameObjectPosition(Vector2Int pos)
    {
        // 指定された位置のタイルの番号を取得
        TileType cell = tileList[pos.x, pos.y];

        // プレイヤーもしくはブロックの場合
        if (cell == TileType.PLAYER || cell == TileType.BLOCK)
        {
            // 地面に変更
            tileList[pos.x, pos.y] = TileType.GROUND;
        }
        // 目的地に乗っているプレイヤーもしくはブロックの場合
        else if (cell == TileType.PLAYER_ON_TARGET || cell == TileType.BLOCK_ON_TARGET)
        {
            // 目的地に変更
            tileList[pos.x, pos.y] = TileType.TARGET;
        }
    }

    // ゲームをクリアしたかどうか確認
    private void CheckCompletion()
    {
        // 目的地に乗っているブロックの数を計算
        int blockOnTargetCount = 0;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (tileList[x, y] == TileType.BLOCK_ON_TARGET)
                {
                    blockOnTargetCount++;
                }
            }
        }

        // すべてのブロックが目的地の上に乗っている場合
        if (blockOnTargetCount == blockCount)
        {
            // ゲームクリア
            isClear = true;
        }
    }
}