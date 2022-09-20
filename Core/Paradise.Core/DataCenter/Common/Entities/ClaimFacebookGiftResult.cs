namespace Paradise.DataCenter.Common.Entities {
	public enum ClaimFacebookGiftResult {
		ErrorUnknown,
		ErrorCouldNotFindRequest,
		ErrorRequestHasInvalidData,
		ErrorCouldNotDeleteRequest,
		ErrorCouldNotGenerateItemId,
		AlreadyOwnedPermanently,
		RentalTimeProlonged,
		NewItemAttributed,
		ErrorWhileSavingItemChanges,
		ErrorClaimerIsNotReceiver
	}
}
