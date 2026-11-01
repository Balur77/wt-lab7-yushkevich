using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace wt_lab7_7postgres_efcore_yushkevich.Models;

/// <summary>
/// Груз на складе — основная сущность предметной области
/// «Склад и логистика», перенесённая из WT-4, WT-5 и WT-6.
/// </summary>
public class Cargo
{
    /// <summary>
    /// Идентификатор груза. Назначается базой данных при создании,
    /// значение из клиента не принимается.
    /// </summary>
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название груза")]
    [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите категорию")]
    [StringLength(60, ErrorMessage = "Категория не должна превышать 60 символов")]
    public string Category { get; set; } = string.Empty;

    [Range(1, 1_000_000, ErrorMessage = "Количество должно быть от 1 до 1 000 000")]
    public int Quantity { get; set; } = 1;

    [Required(ErrorMessage = "Укажите место хранения")]
    [StringLength(100, ErrorMessage = "Место хранения не должно превышать 100 символов")]
    public string Location { get; set; } = string.Empty;

    [Range(0.01, 1_000_000_000, ErrorMessage = "Вес должен быть положительным числом")]
    public decimal Weight { get; set; }

    [Required(ErrorMessage = "Укажите описание груза")]
    [StringLength(300, ErrorMessage = "Описание не должно превышать 300 символов")]
    public string Description { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;

    /// <summary>Дата и время добавления груза в базу данных (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "Укажите поставщика")]
    [StringLength(120, ErrorMessage = "Название поставщика не должно превышать 120 символов")]
    public string Supplier { get; set; } = string.Empty;

    /// <summary>Общий вес партии. Вычисляется в модели и не хранится в PostgreSQL.</summary>
    [NotMapped]
    public decimal TotalWeight => Math.Round(Weight * Quantity, 2);

    /// <summary>Текстовое представление статуса для клиента API. Не хранится в БД.</summary>
    [NotMapped]
    public string Status => IsAvailable ? "На складе" : "Нет в наличии";
}
