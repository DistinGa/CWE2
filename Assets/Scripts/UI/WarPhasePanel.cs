using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarPhasePanel : MonoBehaviour
{
    [SerializeField] GameObject Image, InactiveImage;
    [SerializeField] GameObject StartBattleButton, GoToBattleButton, VictoryImage;
    [SerializeField] GameObject BonusPenaltyPrefab;
    [SerializeField] Transform Bonuses;
}
