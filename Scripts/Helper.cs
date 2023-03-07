using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helper : MonoBehaviour
{
  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {

  }

  static public float GetExpSliderValue(int exp)
  {
    if (PlayerManager.instance.selectedTank == null) return 0;
    int level = PlayerManager.instance.selectedTank.tankLevel;
    float stackExp = level * level * 1000f;
    float needExp = (level + 1) * (level + 1) * 1000f;
    return ((float)exp - stackExp) / (needExp - stackExp);
  }

  static public int[] GetExpValues(int level, int exp)
  {
    int stackExp = level * level * 1000;
    int needExp = (level + 1) * (level + 1) * 1000;
    return (new int[2] { (exp - stackExp), (needExp - stackExp) });
  }

}
