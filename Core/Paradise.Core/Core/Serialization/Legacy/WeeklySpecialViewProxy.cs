using System;
using System.IO;
using Paradise.DataCenter.Common.Entities;

namespace Paradise.Core.Serialization.Legacy
{
	public static class WeeklySpecialViewProxy
	{
		public static void Serialize(Stream stream, WeeklySpecialView instance) {
			int num = 0;
			if (instance != null) {
				using (MemoryStream memoryStream = new MemoryStream()) {
					if (instance.EndDate != null) {
						DateTimeProxy.Serialize(memoryStream, instance.EndDate ?? default(DateTime));
					} else {
						num |= 1;
					}
					Int32Proxy.Serialize(memoryStream, instance.Id);
					if (instance.ImageUrl != null) {
						StringProxy.Serialize(memoryStream, instance.ImageUrl);
					} else {
						num |= 2;
					}
					Int32Proxy.Serialize(memoryStream, instance.ItemId);
					DateTimeProxy.Serialize(memoryStream, instance.StartDate);
					if (instance.Text != null) {
						StringProxy.Serialize(memoryStream, instance.Text);
					} else {
						num |= 4;
					}
					if (instance.Title != null) {
						StringProxy.Serialize(memoryStream, instance.Title);
					} else {
						num |= 8;
					}
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			} else {
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static WeeklySpecialView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			WeeklySpecialView weeklySpecialView = null;
			if (num != 0) {
				weeklySpecialView = new WeeklySpecialView();
				if ((num & 1) != 0) {
					weeklySpecialView.EndDate = new DateTime?(DateTimeProxy.Deserialize(bytes));
				}
				weeklySpecialView.Id = Int32Proxy.Deserialize(bytes);
				if ((num & 2) != 0) {
					weeklySpecialView.ImageUrl = StringProxy.Deserialize(bytes);
				}
				weeklySpecialView.ItemId = Int32Proxy.Deserialize(bytes);
				weeklySpecialView.StartDate = DateTimeProxy.Deserialize(bytes);
				if ((num & 4) != 0) {
					weeklySpecialView.Text = StringProxy.Deserialize(bytes);
				}
				if ((num & 8) != 0) {
					weeklySpecialView.Title = StringProxy.Deserialize(bytes);
				}
			}
			return weeklySpecialView;
		}
	}
}
