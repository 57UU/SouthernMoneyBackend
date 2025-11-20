# 南方财富后端
- 程序入口是`Program.cs`
- 控制器在`SouthernMoneyBackend/Controllers`目录下
- 数据库的服务层位于`Database/xxxService.cs`

依赖通过依赖注入实现，无需手动管理依赖。

# API可视化
访问`/swagger`即可查看API可视化文档

# API细节
## login
### 注册用户
- **路径**: `/login/register`
- **方法**: `POST`
- **参数**:
  - `Name`: 用户名（必填）
  - `Password`: 密码（必填）
- **成功响应**:
  - `200 OK`
- **错误响应**:
  - `400 Bad Request`: 包含错误信息

### 密码登录
- **路径**: `/login/loginByPassword`
- **方法**: `POST`
- **参数**:
  - `Name`: 用户名（必填）
  - `Password`: 密码（必填）
- **成功响应**:
  - `200 OK`: 返回Session对象，包含Token、过期时间等
- **错误响应**:
  - `400 Bad Request`: 包含错误信息（如用户不存在、密码不匹配）

### Token登录
- **路径**: `/login/loginByToken`
- **方法**: `POST`
- **参数**:
  - `Token`: 登录令牌（必填）
- **成功响应**:
  - `200 OK`: 返回Session对象
- **错误响应**:
  - `400 Bad Request`: 包含错误信息

## images
### 上传图片
- **路径**: `/images`
- **方法**: `POST`
- **参数**:
  - `file`: 图片文件（必填，最大2MB）
  - `imageType`: 图片类型（必填）
  - `description`: 图片描述（可选）
- **成功响应**:
  - `200 OK`: 返回`{ "ImageId": "图片ID" }`
- **错误响应**:
  - `400 Bad Request`: 包含错误信息（如文件为空、文件大小超过限制）
  - `401 Unauthorized`: 认证失败

### 获取图片
- **路径**: `/images`
- **方法**: `GET`
- **参数**:
  - `imageId`: 图片ID（必填，Guid格式）
- **成功响应**:
  - `200 OK`: 返回Image对象，包含图片数据和元信息
- **错误响应**:
  - `404 Not Found`: 图片不存在
  - `401 Unauthorized`: 认证失败

## posts


## 认证方式
所有非登录相关的API（即非`/login/*`路径）都需要在请求头中提供以下认证信息：
- `account`: 用户名
- `token`: 登录令牌

认证失败时，API将返回`401 Unauthorized`状态码，并附带错误信息。
