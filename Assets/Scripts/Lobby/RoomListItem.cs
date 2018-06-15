﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking.Match;

public class RoomListItem : MonoBehaviour {

	public delegate void JoinRoomDelegate(MatchInfoSnapshot _match);
	private JoinRoomDelegate joinRoomCallback;

	[SerializeField]
	private Text roomNameText;

	private MatchInfoSnapshot match;

	public void Setup(MatchInfoSnapshot _match, JoinRoomDelegate _joinRoomCallback){
		match = _match;
		joinRoomCallback = _joinRoomCallback;
		roomNameText.text = ""+match.name;
	}

	public void JoinGame(){
		joinRoomCallback.Invoke(match);
	}
}