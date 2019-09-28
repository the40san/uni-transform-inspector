using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace KoganeUnityLibEditor
{
	[CanEditMultipleObjects]
	[CustomEditor( typeof( Transform ) )]
	public sealed class TransformInspector : Editor
	{
		//==============================================================================
		// 定数(const)
		//==============================================================================
		private const BindingFlags SET_LOCAL_EULER_ANGLES_ATTR =
			BindingFlags.Instance |
			BindingFlags.NonPublic;

		private const BindingFlags ROTATION_ATTR =
			BindingFlags.Instance |
			BindingFlags.Public;

		//==============================================================================
		// 定数(static readonly)
		//==============================================================================
		private static readonly object[]   RESET_ROTATION_PARAMETERS = { Vector3.zero, 0 };
		private static readonly GUIContent PROPERTY_FIELD_LABEL      = new GUIContent( string.Empty );
		
		
		private static readonly object[]   INVERSE_ROTATION_X = { new Vector3(180, 0, 0) };
		private static readonly object[]   INVERSE_ROTATION_Y = { new Vector3(0, 180, 0) };
		private static readonly object[]   INVERSE_ROTATION_Z = { new Vector3(0, 0, 180) };

		//==============================================================================
		// 変数
		//==============================================================================
		private SerializedProperty   m_positionProperty;
		private SerializedProperty   m_rotationProperty;
		private SerializedProperty   m_scaleProperty;
		private GUIStyle             m_resetButtonStyle;
		private GUIStyle             m_invButtonStyle;
		private TransformRotationGUI m_transformRotationGUI;
		private MethodInfo           m_setLocalEulerAnglesMethod;
		private MethodInfo           m_rotateMethod;

		//==============================================================================
		// 関数
		//==============================================================================
		/// <summary>
		/// 有効になった時に呼び出されます
		/// </summary>
		private void OnEnable()
		{
			m_positionProperty = serializedObject.FindProperty( "m_LocalPosition" );
			m_rotationProperty = serializedObject.FindProperty( "m_LocalRotation" );
			m_scaleProperty    = serializedObject.FindProperty( "m_LocalScale" );

			if ( m_transformRotationGUI == null )
			{
				m_transformRotationGUI = new TransformRotationGUI();
			}

			m_transformRotationGUI.OnEnable( m_rotationProperty );

			if ( m_setLocalEulerAnglesMethod == null )
			{
				var transformType = typeof( Transform );
				m_setLocalEulerAnglesMethod = transformType.GetMethod
				(
					name: "SetLocalEulerAngles",
					bindingAttr: SET_LOCAL_EULER_ANGLES_ATTR
				);
			}

			if (m_rotateMethod == null)
			{
				var transformType = typeof( Transform );
				m_rotateMethod = transformType.GetMethod
				(
					name: "Rotate",
					bindingAttr: ROTATION_ATTR 
				);
			}
				
		}

		/// <summary>
		/// Inspector の GUI を描画する時に呼び出されます
		/// </summary>
		public override void OnInspectorGUI()
		{
			// リセットボタンの GUI スタイルを作成
			if ( m_resetButtonStyle == null )
			{
				m_resetButtonStyle = new GUIStyle( EditorStyles.toolbarButton )
				{
					fixedHeight = 20,
					fixedWidth  = 20,
				};
			}
			
			if ( m_invButtonStyle == null )
			{
				m_invButtonStyle = new GUIStyle( EditorStyles.toolbarButton )
				{
					fixedHeight = 20,
					fixedWidth  = 50,
				};
			}

			var oldLabelWidth = EditorGUIUtility.labelWidth;

			// プロパティのラベルの表示幅を設定
			if ( !EditorGUIUtility.wideMode )
			{
				EditorGUIUtility.wideMode   = true;
				EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth - 212;
			}

			serializedObject.Update();

			// 位置の入力欄を表示
			using ( new EditorGUILayout.HorizontalScope() )
			{
				// リセットボタン
				if ( GUILayout.Button( "P", m_resetButtonStyle ) )
				{
					m_positionProperty.vector3Value = Vector3.zero;
				}

				// 入力欄
				EditorGUILayout.PropertyField( m_positionProperty, PROPERTY_FIELD_LABEL );
			}

			// 回転角の入力欄を表示
			using ( new EditorGUILayout.HorizontalScope() )
			{
				// リセットボタン
				if ( GUILayout.Button( "R", m_resetButtonStyle ) )
				{
					var targetObjects = m_rotationProperty.serializedObject.targetObjects;

					Undo.RecordObjects( targetObjects, "Inspector" );

					foreach ( var n in targetObjects )
					{
						m_setLocalEulerAnglesMethod.Invoke( n, RESET_ROTATION_PARAMETERS );
					}
				}
				// 入力欄
				m_transformRotationGUI.RotationField();
			}

			// スケーリング値の入力欄を表示
			using ( new EditorGUILayout.HorizontalScope() )
			{
				// リセットボタン
				if ( GUILayout.Button( "S", m_resetButtonStyle ) )
				{
					m_scaleProperty.vector3Value = Vector3.one;
				}

				// 入力欄
				EditorGUILayout.PropertyField( m_scaleProperty, PROPERTY_FIELD_LABEL );
			}

			
			// 回転角の入力欄を表示
			using ( new EditorGUILayout.HorizontalScope() )
			{
				// リセットボタン
				if ( GUILayout.Button( "XInv", m_invButtonStyle ) )
				{
					var targetObjects = m_rotationProperty.serializedObject.targetObjects;

					Undo.RecordObjects( targetObjects, "Inspector" );

					foreach ( var n in targetObjects )
					{
						m_rotateMethod.Invoke( n, INVERSE_ROTATION_X );
					}
				}
				
				if ( GUILayout.Button( "YInv", m_invButtonStyle ) )
				{
					var targetObjects = m_rotationProperty.serializedObject.targetObjects;

					Undo.RecordObjects( targetObjects, "Inspector" );

					foreach ( var n in targetObjects )
					{
						
						m_rotateMethod.Invoke( n, INVERSE_ROTATION_Y );
					}
				}
				
				if ( GUILayout.Button( "ZInv", m_invButtonStyle ) )
				{
					var targetObjects = m_rotationProperty.serializedObject.targetObjects;

					Undo.RecordObjects( targetObjects, "Inspector" );

					foreach ( var n in targetObjects )
					{
						
						m_rotateMethod.Invoke( n, INVERSE_ROTATION_Y );
					}
				}
			}

			
			// 変更を反映
			serializedObject.ApplyModifiedProperties();

			EditorGUIUtility.labelWidth = oldLabelWidth;
		}
	}
}