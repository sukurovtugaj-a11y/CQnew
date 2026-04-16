using System.Collections;
using UnityEngine;

public class PlayerAchievementComponent
{
    private readonly SecMainCharacter owner;

    public PlayerAchievementComponent(SecMainCharacter owner)
    {
        this.owner = owner;
    }

    public IEnumerator CheckAchievementsOnHubSpawn()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        string achievement = GameProgressManager.Instance.GetAndClearPendingAchievement();
        if (!string.IsNullOrEmpty(achievement))
            ShowAchievement(achievement);
    }

    public void ShowAchievement(string upgradeKey)
    {
        GameObject target = upgradeKey switch
        {
            "health" => owner.achievementHealth,
            "upDamage" => owner.achievementUpDamage,
            "doubleJump" => owner.achievementDoubleJump,
            "dash" => owner.achievementDash,
            "checkpoint" => owner.achievementCheckpoint,
            "invincible" => owner.achievementInvincible,
            _ => null
        };

        if (target != null)
            owner.StartCoroutine(ShowAchievementTarget(target));
    }

    private IEnumerator ShowAchievementTarget(GameObject obj)
    {
        var parentPanel = obj.transform.parent?.gameObject;
        if (parentPanel != null && !parentPanel.activeSelf)
            parentPanel.SetActive(true);

        var image = obj.GetComponent<UnityEngine.UI.Image>();
        bool raycastWasEnabled = image != null && image.raycastTarget;
        if (image != null) image.raycastTarget = false;

        obj.SetActive(true);
        yield return new WaitForSeconds(3f);
        obj.SetActive(false);

        if (image != null) image.raycastTarget = raycastWasEnabled;
    }
}
