using System;
using UnityEngine;
public abstract class Observer : MonoBehaviour
{
    public abstract void OnNotify(NotificationType notificationType);
}
