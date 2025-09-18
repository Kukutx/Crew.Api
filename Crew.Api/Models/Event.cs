using System.Collections.Generic;

namespace Crew.Api.Models;

public class Event
{
    // 基本属性
    public int Id { get; set; }
    public string Title { get; set; } = ""; // 活动名称
    public string Type { get; set; } = ""; // 活动类型
    public string Status { get; set; } = ""; // 活动状态
    public string Organizer { get; set; } = ""; // 主办方
    public string Location { get; set; } = ""; // 活动地点
    public string Description { get; set; } = ""; // 活动简介
    public int ExpectedParticipants { get; set; } = 0; // 预计参与人数

    // 时间字段
    public DateTime StartTime { get; set; } // 开始时间
    public DateTime EndTime { get; set; } // 结束时间
    public DateTime CreatedAt { get; set; } // 创建时间
    public DateTime LastUpdated { get; set; } // 最近更新时间

    // 地理位置
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // 图片字段
    public List<string> ImageUrls { get; set; } = new(); // 图片链接列表
    public string CoverImageUrl { get; set; } = ""; // 封面图链接
}
