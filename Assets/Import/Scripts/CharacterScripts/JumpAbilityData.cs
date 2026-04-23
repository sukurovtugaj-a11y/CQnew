using UnityEngine;

[System.Serializable]
public struct JumpAbilityData
{
    public string abilityName;
    public int additionalJumps;
    public float heightMultiplier;
    public Sprite icon;

    public JumpAbilityData(string name, int extraJumps, float heightMult, Sprite abilityIcon = null)
    {
        abilityName = name;
        additionalJumps = extraJumps;
        heightMultiplier = heightMult;
        icon = abilityIcon;
    }
}