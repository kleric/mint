server void ChangeState(string _) { }
server void ChangeState(string _, Creature _) { }
server void ChangeState(string _, Creature _, int _) { }
server void ChangeState(string _, Creature _, int _, bool _) { }
server void GetState() { }
server void SetColor(int _, int _) { }
server void IsNull() { }
server float GetPositionX() { }
server float GetPositionY() { }
server void SetChestKey(int _) { }
server void SetExtraData(string _, string _) { }
server void SetExtraData(string _, string _, int _) { }
server void SetExtraData(string _, float _) { }
server void SetExtraData(string _, int _) { }
server void SetExtraData(string _, long _) { }
server int GetChestKey() { }
server void GetObjectID() { }
server string GetExtraData(string _) { }
server void GetExtraData(string _, long _) { }
server void GetExtraData(string _, string _) { }
server void SetScale(float _, int _) { }
server void SetConditionIndex(int _) { }
server void ShowEffect(string _) { }
server void ShowEffect(Creature _, string _) { }
server void Synch() { }
server void SetCollisionAction(string _) { }
server void GetRegionID() { }
server void GenerateAndDropItem(string _, Creature _) { }
server void GetConditionIndex() { }
server void GetEntityName() { }
server int GetCount(Creature _) { }
server void SetCountDown(int _) { }
server void CheckFastStringID(string _) { }
server bool IsUsableCharacter(Creature _) { }
server void ShowEffectByName(int _, string _) { }
server int GetCountDown() { }
server float GetDirection() { }
server void SetRelatedItem(Item _) { }
server void SetProductionBonus(int _) { }
server void GetProductionBonus() { }
