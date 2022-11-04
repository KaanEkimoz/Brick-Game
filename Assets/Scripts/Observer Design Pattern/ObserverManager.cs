using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObserverManager : MonoBehaviour
{
    #region SINGLETON
    private static ObserverManager instance = null;
    public static ObserverManager Instance => instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    private List<Subject> subjects = null;

    public void RegisterSubject(Subject subject)
    {
        if (subjects == null)
            subjects = new List<Subject>();
        
        subjects.Add(subject);
    }

    public void RegisterObserver(Observer observer, SubjectType subjectType)
    {
        foreach (var subject in subjects)
        {
            if(subject.SubjectType == subjectType)
                subject.RegisterObserver(observer);
        }
    }
}

public enum NotificationType
{
    Landed
}

public enum SubjectType
{
    Grid
}