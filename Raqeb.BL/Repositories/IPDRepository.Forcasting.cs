using EFCore.BulkExtensions;
using OfficeOpenXml;
using Raqeb.Shared.DTOs;
using Raqeb.Shared.ViewModels.Responses;
using Raqeb.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raqeb.BL.Repositories
{
	/// <summary>
	/// 🔹 جزء الـ Forecasting من PDRepository
	/// يقوم بحساب توقعات (Base, Best, Worst) لمعدلات PD المستقبلية
	/// بناءً على بيانات PD التاريخية والعوامل الاقتصادية.
	/// </summary>
	public partial class PDRepository 
	{
		/// <summary>
		/// 🧮 تنبؤ بمعدلات PD المستقبلية لعدد معين من السنوات القادمة.
		/// يحسب 3 سيناريوهات: Base, Best, Worst.
		/// </summary>
	
	}
}
