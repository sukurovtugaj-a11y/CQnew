public class PlayerUpgradeComponent
{
    private readonly SecMainCharacter owner;

    public PlayerUpgradeComponent(SecMainCharacter owner)
    {
        this.owner = owner;
    }

    public void ApplyUpgrades()
    {
        var gpm = GameProgressManager.Instance;
        if (gpm == null) return;

        string train = gpm.GetUpgrade("train", false);
        if (train == "health") { owner.maxHealth = 125; owner.currentHealth = 125; }

        string first = gpm.GetUpgrade("firstLevel", false);
        if (first == "doubleJump") { owner.maxExtraJumps = 1; owner.extraJumpsLeft = 1; }
        if (first == "dash") { owner.canDash = true; } else { owner.canDash = false; }

        string second = gpm.GetUpgrade("secondLevel", false);
        if (second == "checkpoint") { owner.checkpointEnabled = true; } else { owner.checkpointEnabled = false; }
        if (second == "invincible") { owner.canActivateInvulnerability = true; } else { owner.canActivateInvulnerability = false; }

        string train2 = gpm.GetUpgrade("train", true);
        if (train2 == "health") { owner.maxHealth = 125; owner.currentHealth = 125; }

        string first2 = gpm.GetUpgrade("firstLevel", true);
        if (first2 == "doubleJump" && owner.maxExtraJumps < 1) { owner.maxExtraJumps = 1; owner.extraJumpsLeft = 1; }
        if (first2 == "dash") { owner.canDash = true; }

        string second2 = gpm.GetUpgrade("secondLevel", true);
        if (second2 == "checkpoint") { owner.checkpointEnabled = true; }
        if (second2 == "invincible") { owner.canActivateInvulnerability = true; }
    }
}
