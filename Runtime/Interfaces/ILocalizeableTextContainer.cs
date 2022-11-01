using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILocalizeableTextContainer
{
    string ContainerName { get; }
    ILocalizeableText[] GetAllChilds();
}
