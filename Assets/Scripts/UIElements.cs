using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIButton
{
	public          GameObject obj;
	public          Image      img;
	public          Button     btn;

	public UIButton(string name, UnityAction func)
	{
		obj         = GameObject.Find(name);
		img         = obj.GetComponent<Image>();
		btn         = obj.GetComponent<Button>();
		btn.onClick.AddListener(func);
	}
}

public class ToastMsg
{
	public GameObject obj;
	public Image      img;
	public TMP_Text   txt;
}