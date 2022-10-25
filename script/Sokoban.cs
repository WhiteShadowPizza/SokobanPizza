using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Sokoban : MonoBehaviour
{
    // �^�C���̎��
    private enum TileType
    {
        NONE, // ��������
        GROUND, // �n��
        TARGET, // �ړI�n
        PLAYER, // �v���C���[
        BLOCK, // �u���b�N

        PLAYER_ON_TARGET, // �v���C���[�i�ړI�n�̏�j
        BLOCK_ON_TARGET, // �u���b�N�i�ړI�n�̏�j
    }

    // �����̎��
    private enum DirectionType
    {
        UP, // ��
        RIGHT, // �E
        DOWN, // ��
        LEFT, // ��
    }

    public TextAsset stageFile; // �X�e�[�W�\�����L�q���ꂽ�e�L�X�g�t�@�C��

    private int rows; // �s��
    private int columns; // ��
    private TileType[,] tileList; // �^�C�������Ǘ�����񎟌��z��

    public float tileSize; // �^�C���̃T�C�Y

    public Sprite groundSprite; // �n�ʂ̃X�v���C�g
    public Sprite targetSprite; // �ړI�n�̃X�v���C�g
    public Sprite playerSprite; // �v���C���[�̃X�v���C�g
    public Sprite blockSprite; // �u���b�N�̃X�v���C�g

    [SerializeField] private Sprite playerSpriteUp;
    [SerializeField] private Sprite playerSpriteDown;
    [SerializeField] private Sprite playerSpriteRight;
    [SerializeField] private Sprite playerSpriteLeft;
    private SpriteRenderer pSprite;

    private GameObject player; // �v���C���[�̃Q�[���I�u�W�F�N�g
    private Vector2 middleOffset; // ���S�ʒu
    private int blockCount; // �u���b�N�̐�
    private bool isClear; // �Q�[�����N���A�����ꍇ true
    public int counter = 0;
    [SerializeField] private int MaxCount = 0;
    public GameObject score_object = null; // Text�I�u�W�F�N�g
    public int score_num = 0; // �X�R�A�ϐ�
    [SerializeField] Text cleartext;
    [SerializeField] Text gameovertext;
    [SerializeField] bool move;


    // �e�ʒu�ɑ��݂���Q�[���I�u�W�F�N�g���Ǘ�����A�z�z��
    private Dictionary<GameObject, Vector2Int> gameObjectPosTable = new Dictionary<GameObject, Vector2Int>();

    // �Q�[���J�n���ɌĂяo�����
    private void Start()
    {
        LoadTileData(); // �^�C���̏���ǂݍ���
        CreateStage(); // �X�e�[�W���쐬
        cleartext.enabled = false;
        gameovertext.enabled = false;
        move = true;
    }

    // �^�C���̏���ǂݍ���
    private void LoadTileData()
    {
        // �^�C���̏�����s���Ƃɕ���
        var lines = stageFile.text.Split
        (
            new[] { '\r', '\n' },
            StringSplitOptions.RemoveEmptyEntries
        );

        // �^�C���̗񐔂��v�Z
        var nums = lines[0].Split(new[] { ',' });

        // �^�C���̗񐔂ƍs����ێ�
        rows = lines.Length; // �s��
        columns = nums.Length; // ��

        // �^�C������ int �^�̂Q�����z��ŕێ�
        tileList = new TileType[columns, rows];
        for (int y = 0; y < rows; y++)
        {
            // �ꕶ�����擾
            var st = lines[y];
            nums = st.Split(new[] { ',' });
            for (int x = 0; x < columns; x++)
            {
                // �ǂݍ��񂾕����𐔒l�ɕϊ����ĕێ�
                tileList[x, y] = (TileType)int.Parse(nums[x]);
            }
        }
    }

    // �X�e�[�W���쐬
    private void CreateStage()
    {
        // �X�e�[�W�̒��S�ʒu���v�Z
        middleOffset.x = columns * tileSize * 0.5f - tileSize * 0.5f;
        middleOffset.y = rows * tileSize * 0.5f - tileSize * 0.5f; ;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                var val = tileList[x, y];

                // ���������ꏊ�͖���
                if (val == TileType.NONE) continue;

                // �^�C���̖��O�ɍs�ԍ��Ɨ�ԍ���t�^
                var name = "tile" + y + "_" + x;

                // �^�C���̃Q�[���I�u�W�F�N�g���쐬
                var tile = new GameObject(name);

                // �^�C���ɃX�v���C�g��`�悷��@�\��ǉ�
                var sr = tile.AddComponent<SpriteRenderer>();

                // �^�C���̃X�v���C�g��ݒ�
                sr.sprite = groundSprite;

                // �^�C���̈ʒu��ݒ�
                tile.transform.position = GetDisplayPosition(x, y);

                // �ړI�n�̏ꍇ
                if (val == TileType.TARGET)
                {
                    // �ړI�n�̃Q�[���I�u�W�F�N�g���쐬
                    var destination = new GameObject("destination");

                    // �ړI�n�ɃX�v���C�g��`�悷��@�\��ǉ�
                    sr = destination.AddComponent<SpriteRenderer>();

                    // �ړI�n�̃X�v���C�g��ݒ�
                    sr.sprite = targetSprite;

                    // �ړI�n�̕`�揇����O�ɂ���
                    sr.sortingOrder = 1;

                    // �ړI�n�̈ʒu��ݒ�
                    destination.transform.position = GetDisplayPosition(x, y);
                }
                // �v���C���[�̏ꍇ
                if (val == TileType.PLAYER)
                {
                    // �v���C���[�̃Q�[���I�u�W�F�N�g���쐬
                    player = new GameObject("player");

                    // �v���C���[�ɃX�v���C�g��`�悷��@�\��ǉ�
                    sr = player.AddComponent<SpriteRenderer>();

                    // �v���C���[�̃X�v���C�g��ݒ�
                    sr.sprite = playerSprite;

                    // �v���C���[�̕`�揇����O�ɂ���
                    sr.sortingOrder = 2;

                    // �v���C���[�̈ʒu��ݒ�
                    player.transform.position = GetDisplayPosition(x, y);

                    // �v���C���[��A�z�z��ɒǉ�
                    gameObjectPosTable.Add(player, new Vector2Int(x, y));

                    
                }
                // �u���b�N�̏ꍇ
                else if (val == TileType.BLOCK)
                {
                    // �u���b�N�̐��𑝂₷
                    blockCount++;

                    // �u���b�N�̃Q�[���I�u�W�F�N�g���쐬
                    var block = new GameObject("block" + blockCount);

                    // �u���b�N�ɃX�v���C�g��`�悷��@�\��ǉ�
                    sr = block.AddComponent<SpriteRenderer>();

                    // �u���b�N�̃X�v���C�g��ݒ�
                    sr.sprite = blockSprite;

                    // �u���b�N�̕`�揇����O�ɂ���
                    sr.sortingOrder = 2;

                    // �u���b�N�̈ʒu��ݒ�
                    block.transform.position = GetDisplayPosition(x, y);

                    // �u���b�N��A�z�z��ɒǉ�
                    gameObjectPosTable.Add(block, new Vector2Int(x, y));
                }
            }
        }
    }

    // �w�肳�ꂽ�s�ԍ��Ɨ�ԍ�����X�v���C�g�̕\���ʒu���v�Z���ĕԂ�
    private Vector2 GetDisplayPosition(int x, int y)
    {
        return new Vector2
        (
            x * tileSize - middleOffset.x,
            y * -tileSize + middleOffset.y
        );
    }

    // �w�肳�ꂽ�ʒu�ɑ��݂���Q�[���I�u�W�F�N�g��Ԃ��܂�
    private GameObject GetGameObjectAtPosition(Vector2Int pos)
    {
        foreach (var pair in gameObjectPosTable)
        {
            // �w�肳�ꂽ�ʒu�����������ꍇ
            if (pair.Value == pos)
            {
                // ���̈ʒu�ɑ��݂���Q�[���I�u�W�F�N�g��Ԃ�
                return pair.Key;
            }
        }
        return null;
    }

    // �w�肳�ꂽ�ʒu�̃^�C�����u���b�N�Ȃ� true ��Ԃ�
    private bool IsBlock(Vector2Int pos)
    {
        var cell = tileList[pos.x, pos.y];
        return cell == TileType.BLOCK || cell == TileType.BLOCK_ON_TARGET;
    }

    // �w�肳�ꂽ�ʒu���X�e�[�W���Ȃ� true ��Ԃ�
    private bool IsValidPosition(Vector2Int pos)
    {
        if (0 <= pos.x && pos.x < columns && 0 <= pos.y && pos.y < rows)
        {
            return tileList[pos.x, pos.y] != TileType.NONE;
        }
        return false;
    }

    // ���t���[���Ăяo�����
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // ���݂̃V�[�����ēǂݍ���
            int sceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(sceneIndex);
        }

        // �I�u�W�F�N�g����Text�R���|�[�l���g���擾
        Text score_text = score_object.GetComponent<Text>();
        // �e�L�X�g�̕\�������ւ���
        score_text.text = "����:" + counter;

        // �Q�[���N���A���Ă���ꍇ�͑���ł��Ȃ��悤�ɂ���
        if (isClear)
        {
            cleartext.enabled = true;
            move = false;
        }
        if (move == true)
        {
            // ���󂪉����ꂽ�ꍇ
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                // �v���C���[����Ɉړ��ł��邩����
                TryMovePlayer(DirectionType.UP);
            }
            // �E��󂪉����ꂽ�ꍇ
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                // �v���C���[���E�Ɉړ��ł��邩����
                TryMovePlayer(DirectionType.RIGHT);
            }
            // ����󂪉����ꂽ�ꍇ
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                // �v���C���[�����Ɉړ��ł��邩����
                TryMovePlayer(DirectionType.DOWN);
            }
            // ����󂪉����ꂽ�ꍇ
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                // �v���C���[�����Ɉړ��ł��邩����
                TryMovePlayer(DirectionType.LEFT);
            }

            if (counter >= MaxCount)
            {
                move = false;
                gameovertext.enabled = true;
            }

            if (counter >= MaxCount && isClear)
            {
                move = false;
                cleartext.enabled = true;
            }
        }
    }

    // �w�肳�ꂽ�����Ƀv���C���[���ړ��ł��邩����
    // �ړ��ł���ꍇ�͈ړ�����
    private void TryMovePlayer(DirectionType direction)
    {
        // �v���C���[�̌��ݒn���擾
        var currentPlayerPos = gameObjectPosTable[player];

        // �v���C���[�̈ړ���̈ʒu���v�Z
        var nextPlayerPos = GetNextPositionAlong(currentPlayerPos, direction);

        // �v���C���[�̈ړ��悪�X�e�[�W���ł͂Ȃ��ꍇ�͖���
        if (!IsValidPosition(nextPlayerPos)) return;

        // �v���C���[�̈ړ���Ƀu���b�N�����݂���ꍇ
        if (IsBlock(nextPlayerPos))
        {
            // �u���b�N�̈ړ���̈ʒu���v�Z
            var nextBlockPos = GetNextPositionAlong(nextPlayerPos, direction);

            // �u���b�N�̈ړ��悪�X�e�[�W���̏ꍇ����
            // �u���b�N�̈ړ���Ƀu���b�N�����݂��Ȃ��ꍇ
            if (IsValidPosition(nextBlockPos) && !IsBlock(nextBlockPos))
            {
                // �ړ�����u���b�N���擾
                var block = GetGameObjectAtPosition(nextPlayerPos);

                // �v���C���[�̈ړ���̃^�C���̏����X�V
                UpdateGameObjectPosition(nextPlayerPos);

                // �u���b�N���ړ�
                block.transform.position = GetDisplayPosition(nextBlockPos.x, nextBlockPos.y);

                // �u���b�N�̈ʒu���X�V
                gameObjectPosTable[block] = nextBlockPos;

                // �u���b�N�̈ړ���̔ԍ����X�V
                if (tileList[nextBlockPos.x, nextBlockPos.y] == TileType.GROUND)
                {
                    // �ړ��悪�n�ʂȂ�u���b�N�̔ԍ��ɍX�V
                    tileList[nextBlockPos.x, nextBlockPos.y] = TileType.BLOCK;
                    counter++;
                }
                else if (tileList[nextBlockPos.x, nextBlockPos.y] == TileType.TARGET)
                {
                    // �ړ��悪�ړI�n�Ȃ�u���b�N�i�ړI�n�̏�j�̔ԍ��ɍX�V
                    tileList[nextBlockPos.x, nextBlockPos.y] = TileType.BLOCK_ON_TARGET;
                }

                // �v���C���[�̌��ݒn�̃^�C���̏����X�V
                UpdateGameObjectPosition(currentPlayerPos);

                // �v���C���[���ړ�
                player.transform.position = GetDisplayPosition(nextPlayerPos.x, nextPlayerPos.y);

                // �v���C���[�̈ʒu���X�V
                gameObjectPosTable[player] = nextPlayerPos;

                // �v���C���[�̈ړ���̔ԍ����X�V
                if (tileList[nextPlayerPos.x, nextPlayerPos.y] == TileType.GROUND)
                {
                    // �ړ��悪�n�ʂȂ�v���C���[�̔ԍ��ɍX�V
                    tileList[nextPlayerPos.x, nextPlayerPos.y] = TileType.PLAYER;
                    
                }
                else if (tileList[nextPlayerPos.x, nextPlayerPos.y] == TileType.TARGET)
                {
                    // �ړ��悪�ړI�n�Ȃ�v���C���[�i�ړI�n�̏�j�̔ԍ��ɍX�V
                    tileList[nextPlayerPos.x, nextPlayerPos.y] = TileType.PLAYER_ON_TARGET;
                }
            }
        }
        // �v���C���[�̈ړ���Ƀu���b�N�����݂��Ȃ��ꍇ
        else
        {
            // �v���C���[�̌��ݒn�̃^�C���̏����X�V
            UpdateGameObjectPosition(currentPlayerPos);

            // �v���C���[���ړ�
            player.transform.position = GetDisplayPosition(nextPlayerPos.x, nextPlayerPos.y);

            // �v���C���[�̈ʒu���X�V
            gameObjectPosTable[player] = nextPlayerPos;

            // �v���C���[�̈ړ���̔ԍ����X�V
            if (tileList[nextPlayerPos.x, nextPlayerPos.y] == TileType.GROUND)
            {
                // �ړ��悪�n�ʂȂ�v���C���[�̔ԍ��ɍX�V
                tileList[nextPlayerPos.x, nextPlayerPos.y] = TileType.PLAYER;
            }
            else if (tileList[nextPlayerPos.x, nextPlayerPos.y] == TileType.TARGET)
            {
                // �ړ��悪�ړI�n�Ȃ�v���C���[�i�ړI�n�̏�j�̔ԍ��ɍX�V
                tileList[nextPlayerPos.x, nextPlayerPos.y] = TileType.PLAYER_ON_TARGET;
            }
            counter++;
        }

        // �Q�[�����N���A�������ǂ����m�F
        CheckCompletion();
    }

    // �w�肳�ꂽ�����̈ʒu��Ԃ�
    private Vector2Int GetNextPositionAlong(Vector2Int pos, DirectionType direction)
    {
        player = GameObject.Find("player");
        pSprite = player.GetComponent<SpriteRenderer>();

        switch (direction)
        {
            // ��
            case DirectionType.UP:
                pSprite.sprite = playerSpriteUp;
                pos.y -= 1;
                break;

            // �E
            case DirectionType.RIGHT:
                pSprite.sprite = playerSpriteRight;
                pos.x += 1;
                break;

            // ��
            case DirectionType.DOWN:
                pSprite.sprite = playerSpriteDown;
                pos.y += 1;
                break;

            // ��
            case DirectionType.LEFT:
                pSprite.sprite = playerSpriteLeft;
                pos.x -= 1;
                break;
        }
        return pos;
    }

    // �w�肳�ꂽ�ʒu�̃^�C�����X�V
    private void UpdateGameObjectPosition(Vector2Int pos)
    {
        // �w�肳�ꂽ�ʒu�̃^�C���̔ԍ����擾
        var cell = tileList[pos.x, pos.y];

        // �v���C���[�������̓u���b�N�̏ꍇ
        if (cell == TileType.PLAYER || cell == TileType.BLOCK)
        {
            // �n�ʂɕύX
            tileList[pos.x, pos.y] = TileType.GROUND;
        }
        // �ړI�n�ɏ���Ă���v���C���[�������̓u���b�N�̏ꍇ
        else if (cell == TileType.PLAYER_ON_TARGET || cell == TileType.BLOCK_ON_TARGET)
        {
            // �ړI�n�ɕύX
            tileList[pos.x, pos.y] = TileType.TARGET;
        }
    }

    // �Q�[�����N���A�������ǂ����m�F
    private void CheckCompletion()
    {
        // �ړI�n�ɏ���Ă���u���b�N�̐����v�Z
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

        // ���ׂẴu���b�N���ړI�n�̏�ɏ���Ă���ꍇ
        if (blockOnTargetCount == blockCount)
        {
            // �Q�[���N���A
            isClear = true;
        }
    }
}
