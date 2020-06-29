using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AgentType{PLAYER,ENEMY};
public class BattleGameArea : MonoBehaviour
{
    [HideInInspector]
    public int _playerLifes = 3;
    [HideInInspector]
    public int _enemyLifes = 3;

    public int MAX_NUM_UNITS = 20;

    public GameObject _playerLifesGO;
    public GameObject _enemyLifesGO;

    public GameObject _enemy, _player;

    public GameObject[] _playerUnits;
    public GameObject[] _enemyUnits;

    // Start is called before the first frame update
    void Start()
    {
        _playerUnits = new GameObject[MAX_NUM_UNITS];
        _enemyUnits = new GameObject[MAX_NUM_UNITS];
        Restart();
    }

    void setLife(int lifes, GameObject go)
    {
        RectTransform rect = go.transform.Find("Fill").gameObject.GetComponent<RectTransform>();
        GameObject heart1 = go.transform.Find("Heart1").gameObject;
        GameObject heart2 = go.transform.Find("Heart2").gameObject;
        GameObject heart3 = go.transform.Find("Heart3").gameObject;

        if (lifes == 0)
        {
            rect.sizeDelta = new Vector2(0, rect.sizeDelta.y);
            heart1.SetActive(false);
            heart2.SetActive(false);
            heart3.SetActive(false);
        }else if (lifes == 1)
        {
            rect.sizeDelta = new Vector2(20, rect.sizeDelta.y);
            heart1.SetActive(true);
            heart2.SetActive(false);
            heart3.SetActive(false);
        }
        else if (lifes == 2)
        {
            rect.sizeDelta = new Vector2(207, rect.sizeDelta.y);
            heart1.SetActive(true);
            heart2.SetActive(true);
            heart3.SetActive(false);
        }
        else if (lifes == 3)
        {
            rect.sizeDelta = new Vector2(414, rect.sizeDelta.y);
            heart1.SetActive(true);
            heart2.SetActive(true);
            heart3.SetActive(true);
        }
    }

    void Restart()
    {
        _playerLifes = 3;
        _enemyLifes = 3;
        setLife(_playerLifes, _playerLifesGO);
        setLife(_enemyLifes, _enemyLifesGO);

        foreach (GameObject go in _playerUnits)
        {
            if(go != null)
                Destroy(go);
        }
        foreach (GameObject go in _playerUnits)
        {
            if (go != null)
                Destroy(go);
        }
        _playerUnits = new GameObject[MAX_NUM_UNITS];
        _enemyUnits = new GameObject[MAX_NUM_UNITS];
    }

    public void unitKilled(AgentType agent, GameObject monster)
    {
        bool found = false;
        switch (agent)
        {
            case AgentType.ENEMY:
                for (int i = 0; i < _enemyUnits.Length; i++)
                {
                    if (_enemyUnits[i] == null)
                    {
                        break;
                    }
                    else if (found && i < _enemyUnits.Length - 1)
                    {
                        _enemyUnits[i] = _enemyUnits[i + 1];
                    }
                    else if (GameObject.ReferenceEquals(_enemyUnits[i], monster))
                    {
                        if (i < _enemyUnits.Length - 1)
                        {
                            _enemyUnits[i] = _enemyUnits[i + 1];
                        }
                        else
                        {
                            _enemyUnits[i] = null;
                        }
                        found = true;
                    }
                }
                break;
            case AgentType.PLAYER:
                for (int i = 0; i < _playerUnits.Length; i++)
                {
                    if (_playerUnits[i] == null)
                    {
                        break;
                    }
                    else if (found && i < _playerUnits.Length - 1)
                    {
                        _playerUnits[i] = _playerUnits[i + 1];
                    }
                    else if (ReferenceEquals(_playerUnits[i], monster))
                    {
                        if (i < _playerUnits.Length - 1)
                        {
                            _playerUnits[i] = _playerUnits[i + 1];
                        }
                        else
                        {
                            _playerUnits[i] = null;
                        }
                        found = true;
                    }
                }
                break;
        }

        _enemy.GetComponent<BattleGameAgent>().unitKilled(agent == AgentType.ENEMY);
        _player.GetComponent<BattleGameAgent>().unitKilled(agent == AgentType.PLAYER);

    }

    public void hitBase(AgentType agent)
    {
        bool end = true;
        if (agent == AgentType.ENEMY)
        {
            _playerLifes--;
            setLife(_playerLifes, _playerLifesGO);
            end = _playerLifes <= 0;
        }
        else
        {
            _enemyLifes--;
            setLife(_enemyLifes, _enemyLifesGO);
            end = _enemyLifes <= 0;
        }
        _enemy.GetComponent<BattleGameAgent>().hitted(agent == AgentType.ENEMY, end);
        _player.GetComponent<BattleGameAgent>().hitted(agent == AgentType.PLAYER, end);
        if (end)
        {
            Restart();
        }
    }

    public bool getOppositeArrowsDirection(AgentType agent)
    {
        bool res = true;
        switch (agent)
        {
            case AgentType.ENEMY:
                res = Mathf.Abs(_player.GetComponent<BattleGameAgent>()._directionArrow.transform.localRotation.z) > 0.1f;
                break;
            case AgentType.PLAYER:
                res = Mathf.Abs(_enemy.GetComponent<BattleGameAgent>()._directionArrow.transform.localRotation.z) > 0.1f;
                break;
        }
        return res;
    }

    public void AddMonster(AgentType agent, GameObject monster)
    {
        switch (agent)
        {
            case AgentType.ENEMY:
                for (int i = 0; i < _enemyUnits.Length; i++)
                {
                    if (_enemyUnits[i] == null)
                    {
                        _enemyUnits[i] = monster;
                        break;
                    }
                }
                break;
            case AgentType.PLAYER:
                for (int i = 0; i < _playerUnits.Length; i++)
                {
                    if (_playerUnits[i] == null)
                    {
                        _playerUnits[i] = monster;
                        break;
                    }
                }
                break;
        }
    }

    public float[,] getUnits(AgentType agent)
    {
        float[,] res = new float[MAX_NUM_UNITS, 3];
        for (int i = 0; i < MAX_NUM_UNITS; i++)
        {
            res[i, 0] = -1;
            res[i, 1] = 0;
            res[i, 2] = 0;
        }
        switch (agent)
        {
            case AgentType.ENEMY:
                for (int i = 0; i < _enemyUnits.Length; i++)
                {
                    if (_enemyUnits[i] != null)
                    {
                        res[i, 0] = (float)_enemyUnits[i].GetComponent<UnitBehaviour>()._type;
                        res[i, 0] = _enemyUnits[i].transform.localPosition.x;
                        res[i, 0] = _enemyUnits[i].transform.localPosition.z;
                    }
                }
                break;
            case AgentType.PLAYER:
                for (int i = 0; i < _playerUnits.Length; i++)
                {
                    if(_playerUnits[i] != null)
                    {
                        res[i, 0] = (float)_playerUnits[i].GetComponent<UnitBehaviour>()._type;
                        res[i, 0] = _playerUnits[i].transform.localPosition.x;
                        res[i, 0] = _playerUnits[i].transform.localPosition.z;
                    }
                }
                break;
        }
        return res;
    }
}
