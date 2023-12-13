using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopup_TournamentMessage : UIPopup
{
    public Transform inTheMoney;
    public Transform final;

    public override void Init(params object[] args)
    {
        isBlurOff = false;
        base.Init(args);

        MPSC_T_StatusInfo.eType type = (MPSC_T_StatusInfo.eType)args[0];

        inTheMoney.SetActive(type == MPSC_T_StatusInfo.eType.eSI_ITM);
        final.SetActive(type == MPSC_T_StatusInfo.eType.eSI_FINAL_TABLE);

        Invoke(nameof(Close), 3f);
    }
}
