﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace EasyUI.Dialogs {

	public class ConfirmDialogUI : MonoBehaviour {

		// UI references ------------------------------------------------
		[SerializeField] private GameObject uiCanvas;
		[SerializeField] private Button uiCloseButton;
		[SerializeField] private TextMeshProUGUI uiTitleText;
		[SerializeField] private TextMeshProUGUI uiMessageText;
		[SerializeField] private GameObject uiButtonsParent;
		[SerializeField] private Button uiNegativeButton;
		[SerializeField] private Button uiPositiveButton;

		public TextMeshProUGUI uiNegativeButtonText;
		public TextMeshProUGUI uiPositiveButtonText;
		public Image uiNegativeButtonImage;
		public Image uiPositiveButtonImage;
		public CanvasGroup uiCanvasGroup;


		// Dialog properties ---------------------------------------------
		//Default values:
		[Space ( 20f )]
		[Header ( "Dialog's defaults:" )]
		[SerializeField] private int maxMessageLetters = 300;
		[SerializeField] private bool _defaultHasButtons = true;
		[SerializeField] private string _defaultNegativeButtonText = "no";
		[SerializeField] private string _defaultPositiveButtonText = "yes";
		[SerializeField] private DialogButtonColor _defaultButtonsColor = DialogButtonColor.Black;
		[SerializeField] private float _defaultFadeDuration = .15f;

		public Queue<Dialog> dialogsQueue = new( );
		public Dialog dialog, tempDialog;

		[Space ( 20f )]
		[SerializeField] private Color[] dialogButtonColors;

		[HideInInspector] public bool IsActive = false;

		// Singleton instance ----------------------------------------------
		public static ConfirmDialogUI Instance;

		private void Awake ( ) {
			if(Instance == null) {
				Instance = this;
			}

			uiNegativeButtonText = uiNegativeButton.GetComponentInChildren <TextMeshProUGUI> ( );
			uiPositiveButtonText = uiPositiveButton.GetComponentInChildren <TextMeshProUGUI> ( );
			uiNegativeButtonImage = uiNegativeButton.GetComponent <Image> ( );
			uiPositiveButtonImage = uiPositiveButton.GetComponent <Image> ( );
			uiCanvasGroup = uiCanvas.GetComponent <CanvasGroup> ( );

			ResetDialog ( );
		}


		//---------------------------------------------------------------------------
		public ConfirmDialogUI SetTitle ( string title ) {
			dialog.Title = title;
			return Instance;
		}

		public ConfirmDialogUI SetMessage ( string message ) {
			dialog.Message = message;
			return Instance;
		}

		public ConfirmDialogUI SetButtonsVisibility ( bool visibility = true ) {
			dialog.HasButtons = visibility;
			return Instance;
		}

		public ConfirmDialogUI SetButtonsColor ( DialogButtonColor color = DialogButtonColor.Black ) {
			dialog.ButtonsColor = color;
			return Instance;
		}

		public ConfirmDialogUI SetNegativeButtonText ( string text = "no" ) {
			dialog.NegativeButtonText = text;
			return Instance;
		}

		public ConfirmDialogUI SetPositiveButtonText ( string text = "yes" ) {
			dialog.PositiveButtonText = text;
			return Instance;
		}

		public ConfirmDialogUI SetFadeDuration ( float duration = .15f ) {
			dialog.FadeDuration = duration;
			return Instance;
		}

		public ConfirmDialogUI OnCloseButtonClicked ( UnityAction action ) {
			dialog.CloseButtonClickAction = action;
			return Instance;
		}

		public ConfirmDialogUI OnNegativeButtonClicked ( UnityAction action ) {
			dialog.NegativeButtonClickAction = action;
			return Instance;
		}

		public ConfirmDialogUI OnPositiveButtonClicked ( UnityAction action ) {
			dialog.PositiveButtonClickAction = action;
			return Instance;
		}


		//---------------------------------------------------------------------------


		public void Show ( ) {
			dialogsQueue.Enqueue ( dialog );
			ResetDialog ( );

			if ( !IsActive )
				FillDialogAndShow ( );
		}

		private void FillDialogAndShow ( ) {
			tempDialog = dialogsQueue.Dequeue ( );

			// Data validation
			if ( string.IsNullOrEmpty ( tempDialog.Message.Trim ( ) ) ) {
				Debug.LogError ( "[DialogUI] Dialog's text can't be empty... use <b>.SetMessage(...)</b>" );
				return;
			}

			uiTitleText.text = tempDialog.Title;

			//trim text
			if ( tempDialog.Message.Length > maxMessageLetters )
				uiMessageText.text = tempDialog.Message.Substring ( 0, maxMessageLetters - 3 ) + "...";
			else
				uiMessageText.text = tempDialog.Message;

			uiButtonsParent.SetActive ( tempDialog.HasButtons );

			uiNegativeButtonText.text = tempDialog.NegativeButtonText;
			uiPositiveButtonText.text = tempDialog.PositiveButtonText;

			Color transparentColor = dialogButtonColors [ ( int )tempDialog.ButtonsColor ];
			transparentColor.a = .12f;
			uiNegativeButtonImage.color = transparentColor;
			uiPositiveButtonImage.color = dialogButtonColors [ ( int )tempDialog.ButtonsColor ];

			IsActive = true;
			uiCanvas.SetActive ( true );
			StartCoroutine ( FadeIn ( tempDialog.FadeDuration ) );
		}

		public void Hide ( ) {
			uiCloseButton.onClick.RemoveAllListeners ( );
			uiNegativeButton.onClick.RemoveAllListeners ( );
			uiPositiveButton.onClick.RemoveAllListeners ( );

			IsActive = false;
			StopAllCoroutines ( );
			StartCoroutine ( FadeOut ( tempDialog.FadeDuration ) );
		}

		private void ResetDialog ( ) {
			dialog = new Dialog ( );

			dialog.FadeDuration = _defaultFadeDuration;
			dialog.HasButtons = _defaultHasButtons;
			dialog.ButtonsColor = _defaultButtonsColor;
			dialog.PositiveButtonText = _defaultPositiveButtonText;
			dialog.NegativeButtonText = _defaultNegativeButtonText;
		}


		//---------------------------------------------------------------------------

		private void InvokeEventAndHideDialog ( UnityAction action ) {
			if ( action != null )
				action.Invoke ( );

			Hide ( );
		}

		//---------------------------------------------------------------------------
		private IEnumerator Fade ( CanvasGroup cGroup, float startAlpha, float endAlpha, float duration ) {
			if ( startAlpha == endAlpha ) {
				Debug.LogError ( "Fade() : startAlpha must be different than endAlpha" );
				yield break;
			}

			float startTime = Time.time;
			float alpha = startAlpha;

			if ( duration > 0f ) {
				//Anim start
				while ( alpha != endAlpha ) {
					alpha = Mathf.Lerp ( startAlpha, endAlpha, (Time.time - startTime) / duration );
					cGroup.alpha = alpha;

					yield return null;
				}

			} else
				cGroup.alpha = endAlpha;

			yield break;
		}

		private IEnumerator FadeIn ( float duration ) {
			// Anim start
			yield return Fade ( uiCanvasGroup, 0f, 1f, duration );

			// Anim end
			uiCloseButton.onClick.AddListener ( ( ) => InvokeEventAndHideDialog ( tempDialog.CloseButtonClickAction ) );
			uiNegativeButton.onClick.AddListener ( ( ) => InvokeEventAndHideDialog ( tempDialog.NegativeButtonClickAction ) );
			uiPositiveButton.onClick.AddListener ( ( ) => InvokeEventAndHideDialog ( tempDialog.PositiveButtonClickAction ) );
		}

		private IEnumerator FadeOut ( float duration ) {
			// Anim start
			yield return Fade ( uiCanvasGroup, 1f, 0f, duration );

			//Anim end
			if ( dialogsQueue.Count != 0 )
				FillDialogAndShow ( );
			else
				uiCanvas.SetActive ( false );
		}

	}

}
