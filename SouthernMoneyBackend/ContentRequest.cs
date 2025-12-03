using Database;
//image part by hr
using System;
using System.Collections.Generic;
//image part finished

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
public class RegisterRequest
{
    public string Name { get; set; }
    public string Password { get; set; }
}
public class LoginByPasswordRequest
{
    public string Name { get; set; }
    public string Password { get; set; }
}
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; }
}
public class PostRequest{
    public string Title { get; set; }
    public string Content { get; set; }
    public ICollection<string> ImageIds { get; set; }
    public ICollection<string> Tags { get; set; }
}

public class PostReportRequest{
    public Guid PostId { get; set; }
    public string Reason { get; set; }
}

// Image Part by HR
//上传图片
public class UploadImageRequest
{
    public byte[] File { get; set; } // 图片文件（必填，最大2MB）
    public string ImageType { get; set; } // 图片类型（必填）
    public string? Description { get; set; } // 图片描述（可选）
}
//获取图片
public class GetImageRequest
{
    public Guid ImageId { get; set; } // 图片ID（必填，Guid格式）
}

// Image Part finished by hr
// Post Part by hr (some are already written by 57u)
// 删除帖子请求
public class DeletePostRequest
{
    public Guid PostId { get; set; } // 帖子ID
}

// 编辑帖子请求
public class EditPostRequest
{
    public Guid PostId { get; set; } // 帖子ID
    public string Title { get; set; } // 新标题
    public string Content { get; set; } // 新内容
    public ICollection<string> ImageIds { get; set; } // 新图片ID列表
    public ICollection<string> Tags { get; set; } // 新标签列表
}
// Post finished by hr
// User by hr

// 更新用户信息请求
public class UpdateUserProfileRequest
{
    public string Name { get; set; } // 用户名
    public string Email { get; set; } // 用户邮箱
}

// 修改密码请求
public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } // 当前密码
    public string NewPassword { get; set; } // 新密码
}

// 上传头像请求
public class UploadAvatarRequest
{
    public byte[] File { get; set; } // 图片文件（必填，最大2MB）
}

// 充值请求
public class TopUpRequest
{
    public decimal Amount { get; set; } // 充值金额
}
//User Part Finished by hr
// transaction by hr 
// 购买商品请求
public class BuyProductRequest
{
    public Guid ProductId { get; set; }
}

// 发布商品请求
public class PublishProductRequest
{
    public string Name { get; set; } // 商品名称
    public decimal Price { get; set; } // 商品价格
    public string Description { get; set; } // 商品描述
    public Guid CategoryId { get; set; } // 商品分类ID
}

// 删除商品请求
public class DeleteProductRequest
{
    public Guid ProductId { get; set; } // 商品ID
}

// 添加分类请求
public class CreateCategoryRequest
{
    public string Category { get; set; } // 商品分类名称
    public string Cover { get; set; } // 分类封面图片ID
}

public class HandleUserRequest
{
    public long UserId { get; set; }
    public bool IsBlocked { get; set; }
    public string? HandleReason { get; set; }
}

public class SetAdminRequest
{
    public long UserId { get; set; }
}

public class HandleReportRequest
{
    public Guid PostId { get; set; }
    public bool IsBlocked { get; set; }
    public string? HandleReason { get; set; }
}

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。


