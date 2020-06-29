using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class BattleGameAgent : Agent
{
    const int _manaMax = 10;

    Dictionary<Monster, KeyValuePair<string, int>> _monstersSTS = new Dictionary<Monster, KeyValuePair<string, int>>
    {
        { Monster.SCISSORS, new KeyValuePair<string, int>("Scissors", 3) },
        { Monster.ROCK, new KeyValuePair<string, int>( "Rock", 3) },
        { Monster.PAPER, new KeyValuePair<string, int>( "Paper", 3) },
        { Monster.LIZARD, new KeyValuePair<string, int>( "Lizard", 5) },
        { Monster.SPOKE, new KeyValuePair<string, int>( "Spoke", 5) }
    };

    int _mana = 0;

    public AgentType _type;
    public GameObject _area;
    BattleGameArea _battleGameArea;

    [Header("Icons")]
    public Sprite _scissorIcon;
    public Sprite _rockIcon;
    public Sprite _paperIcon;
    public Sprite _lizardIcon;
    public Sprite _spokeIcon;

    public GameObject _buttonsMenu;
    public GameObject[] _monstersGO = new GameObject[5];
    public GameObject[] _cardsGO = new GameObject[3];

    public GameObject _respawnZone;

    Monster[] _monstersAvailable = new Monster[3];
    public GameObject _manaGO;
    public GameObject _directionArrow;


    public override void Initialize()
    {
        base.Initialize();
        _battleGameArea = _area.GetComponent<BattleGameArea>();
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        Reset();
    }

    public override void Heuristic(float[] actionsOut)
    {        
        if (_type == AgentType.ENEMY)
        {
            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                actionsOut[0] = 0f;
            }
            else if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                actionsOut[0] = 1f;
            }
            else if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                actionsOut[0] = 2f;
            }
            else if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                actionsOut[0] = 3f;
            }
            else
            {
                actionsOut[0] = 4f;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                actionsOut[0] = 0f;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                actionsOut[0] = 1f;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                actionsOut[0] = 2f;
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                actionsOut[0] = 3f;
            }
            else
            {
                actionsOut[0] = 4f;
            }
        }
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        int action = (int)vectorAction[0];
        if (action < 3)
        {
            if (_monstersSTS[_monstersAvailable[action]].Value <= _mana)
            {
                clickCard(action);
            }
            else
            {
                AddReward(-.05f);
            }
        }
        else if(action == 3)
        {
            rotateDirection();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Current Mana (1 observation)
        sensor.AddObservation(_mana);

        // current cards + cards mana (18 observations)
        for (int i = 0; i < _monstersAvailable.Length; i++) 
        {
            sensor.AddOneHotObservation((int) _monstersAvailable[i], 5);
            sensor.AddObservation(_monstersSTS[_monstersAvailable[i]].Value);
        }

        // lifes remaining (2 observations)
        if (_type == AgentType.PLAYER)
        {
            sensor.AddObservation(_battleGameArea._playerLifes);
            sensor.AddObservation(_battleGameArea._enemyLifes);
        }
        else
        {
            sensor.AddObservation(_battleGameArea._enemyLifes);
            sensor.AddObservation(_battleGameArea._playerLifes);
        }

        // Direction (2 observations)
        sensor.AddObservation(Mathf.Abs(_directionArrow.transform.localRotation.z) > 0.1f);
        sensor.AddObservation(_battleGameArea.getOppositeArrowsDirection(_type));

        // Get Units (40 units * (2 x, z + (1 type * 5 posibilities))) = 280)
        float[,] units = _battleGameArea.getUnits(_type);
        for (int i = 0; i < _battleGameArea.MAX_NUM_UNITS; i++)
        {
            sensor.AddOneHotObservation((int)units[i, 0], 5);
            sensor.AddObservation(units[i, 1]);
            sensor.AddObservation(units[i, 2]);
        }
        units = _battleGameArea.getUnits(_type == AgentType.PLAYER ? AgentType.ENEMY : AgentType.PLAYER);
        for (int i = 0; i < _battleGameArea.MAX_NUM_UNITS; i++)
        {
            sensor.AddOneHotObservation((int)units[i, 0], 5);
            sensor.AddObservation(units[i, 1]);
            sensor.AddObservation(units[i, 2]);
        }

        // Total 303 observations
    }

    public void rotateDirection()
    {
        _directionArrow.transform.Rotate(0.0f, 0.0f, 180.0f, Space.Self);
    }

    public void clickCard(int pos)
    {
        List<int> values = new List<int>() { 0, 1, 2, 3, 4 };
        GameObject gm = Instantiate(_monstersGO[(int)_monstersAvailable[pos]], Vector3.zero, Quaternion.identity);
        gm.name = _monstersSTS[_monstersAvailable[pos]].Key;
        gm.GetComponent<UnitBehaviour>().setDirection(_area, _directionArrow, _type);

        _battleGameArea.AddMonster(_type, gm);

        setMana(_mana - _monstersSTS[_monstersAvailable[pos]].Value);
        for (int i = 0; i < _monstersAvailable.Length; i++)
        {
            if (i != pos)
            {
                values.Remove((int)_monstersAvailable[i]);
            }
        }
        setMonsterCard(pos, (Monster)values[Random.Range(0, values.Count)]);
    }

    void setMonsterCard(int pos, Monster value)
    {
        _monstersAvailable[pos] = value;
        if (_buttonsMenu != null && _buttonsMenu.activeSelf)
        {
            GameObject title = _cardsGO[pos].transform.Find("Title").gameObject;
            GameObject character = _cardsGO[pos].transform.Find("Panel_Character").Find("Icon").gameObject;
            GameObject cost = _cardsGO[pos].transform.Find("Cost").gameObject;

            KeyValuePair<string, int> key_value = _monstersSTS[value];

            title.GetComponent<UnityEngine.UI.Text>().text = key_value.Key;
            cost.GetComponent<UnityEngine.UI.Text>().text = key_value.Value.ToString();

            Sprite icon = null;

            switch (value)
            {
                case Monster.SCISSORS: icon = _scissorIcon; break;
                case Monster.ROCK: icon = _rockIcon; break;
                case Monster.PAPER: icon = _paperIcon; break;
                case Monster.LIZARD: icon = _lizardIcon; break;
                case Monster.SPOKE: icon = _spokeIcon; break;
            }
            character.GetComponent<UnityEngine.UI.Image>().sprite = icon;
        }
    }

    void setMana(int newMana)
    {
        _mana = newMana;
        for (int i = 0; i < _manaGO.transform.childCount; i++)
        {
            if (_mana > i)
            {
                _manaGO.transform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                _manaGO.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        for (int i = 0; i < _monstersAvailable.Length; i++)
        {
            if (_monstersSTS[_monstersAvailable[i]].Value > _mana)
            {
                _cardsGO[i].GetComponent<UnityEngine.UI.Button>().interactable = false;
            }
            else
            {
                _cardsGO[i].GetComponent<UnityEngine.UI.Button>().interactable = true;
            }
        }
    }

    public void Reset()
    {
        CancelInvoke();
        List<int> values = new List<int>() { 0, 1, 2, 3, 4 };
        int index = Random.Range(0, values.Count);
        setMonsterCard(0, (Monster)values[index]);
        values.RemoveAt(index);
        index = Random.Range(0, values.Count);
        setMonsterCard(1, (Monster)values[index]);
        values.RemoveAt(index);
        index = Random.Range(0, values.Count);
        setMonsterCard(2, (Monster)values[index]);

        setMana(_manaMax / 2);
        InvokeRepeating("addMana", 2, 2);
    }

    void addMana()
    {
        if(_mana < _manaMax)
            setMana(_mana + 1);
    }

    public void hitted(bool own, bool end)
    {
        AddReward((own ? 1 : -1) * .5f);
        if (end)
        {
            AddReward((own ? 1 : -1) * 1f);
            EndEpisode();
        }
    }

    public void unitKilled(bool own)
    {
        AddReward((own ? -1 : 1) * .2f);
    }
}
