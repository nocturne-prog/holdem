using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UIPopup_ChangeProfile : UIPopup
{
    public RectTransform content;
    public ToggleGroupEx group;

    public override void Init(params object[] args)
    {
        base.Init(args);

        LMSC_LobbyAvatarList list = (LMSC_LobbyAvatarList)args[0];
        Item_ChangeProfile prefab = ResourceManager.Load<Item_ChangeProfile>(Const.PROFILECHANGE_ITEM);

        for (int i = 0; i < list.bOrderCount; i++)
        {
            int index = i;
            int name = list.avatarList[index].nAvatarSeq;

            Item_ChangeProfile item = Instantiate(prefab, content);

            item.transform.localPosition = Vector3.zero;
            item.avatar.sprite = ResourceManager.LoadAvatar(name.ToString());
            item.toggle.interactable = list.avatarList[index].nAvatarType == 1;

            if (name == GameData.Player.AvatarIndex)
                item.toggle.isOn = true;
            else
                item.toggle.isOn = false;

            group.RegisterToggle(item.toggle);
            item.toggle.group = group;

            item.toggle.onValueChanged.AddListener((active) =>
            {
                if (active)
                {
                    OnClickAvatar(name);
                }
            });
        }
    }

    public void OnClickAvatar(int avatarIndex)
    {
        LMCS_AvatarChangeReq packet = new LMCS_AvatarChangeReq();
        packet.HWord = FPPacket_LSMC.HPD_LM_DATA;
        packet.LWord = FPPacket_LSMC.LMCS_LOBBY_AVATAR_CHANGE_REQ_TAG;
        packet.nAvatarSeqNo = avatarIndex;
        NetworkManager.Lobby.Send(packet);

        GameData.Player.AvatarIndex = avatarIndex;
        LobbyManager.a.UpdateProfle();
    }
}
