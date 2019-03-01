using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class MessageSystem : MonoBehaviour
{
 	[SerializeField] private ScrollRect myScroll = null;
	[SerializeField] private TMPro.TextMeshProUGUI msgPrefab = null;

	public void NewMessage(string msg) {
		TMPro.TextMeshProUGUI d = Instantiate(msgPrefab.gameObject, myScroll.content.transform).GetComponent<TMPro.TextMeshProUGUI>();
		d.gameObject.SetActive(true);
		d.text = msg;
	}
}