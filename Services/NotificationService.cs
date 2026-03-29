using System;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;

namespace TaskReminder.Services;

/// <summary>
/// Windows Toast 通知服务
/// 提供系统级通知功能
/// </summary>
public class NotificationService
{
    /// <summary>
    /// 显示 Toast 通知
    /// </summary>
    /// <param name="title">通知标题</param>
    /// <param name="message">通知内容</param>
    public void ShowNotification(string title, string message)
    {
        try
        {
            // 获取 Toast 模板
            var toastTemplate = ToastTemplateType.ToastText02;
            var toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

            // 设置标题和文本
            var toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(title));
            toastTextElements[1].AppendChild(toastXml.CreateTextNode(message));

            // 设置点击行为：打开应用
            var toastNode = toastXml.SelectSingleNode("/toast");
            if (toastNode != null)
            {
                var bindingNode = toastXml.SelectSingleNode("/toast/visual/binding");
                if (bindingNode != null)
                {
                    // 设置通知图标
                    var imageNode = toastXml.CreateElement("image");
                    imageNode.SetAttribute("src", "ms-appx:///Assets/icon.png");
                    imageNode.SetAttribute("placement", "appLogoOverride");
                    bindingNode.AppendChild(imageNode);
                }
            }

            // 创建 Toast 通知
            var toast = new ToastNotification(toastXml);

            // 设置标签（用于分组和管理）
            toast.Tag = "task-reminder";
            toast.Group = "task-reminder-group";

            // 发送通知
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
        catch (Exception ex)
        {
            // 如果 Toast 通知失败，使用控制台输出作为降级
            Console.WriteLine($"[通知] {title}: {message}");
            Console.WriteLine($"Toast 通知失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 显示简单通知（控制台版本，用于调试）
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="message">消息</param>
    public void ShowSimpleNotification(string title, string message)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {title}: {message}");
    }
}
