(function() {
    // 等待Swagger UI完全加载
    window.addEventListener('load', function() {
        // 设置一个定时器，确保Swagger UI完全初始化
        setTimeout(function() {
            try {
                // 登录函数
                async function loginAndSetToken() {
                    try {
                        // 登录请求
                        const loginResponse = await fetch('/login/loginByPassword', {
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/json'
                            },
                            body: JSON.stringify({
                                name: 'test',
                                password: '123'
                            })
                        });
                        
                        if (!loginResponse.ok) {
                            console.error('登录失败:', loginResponse.statusText);
                            return;
                        }
                        
                        const loginData = await loginResponse.json();
                        console.log('登录响应:', loginData); // 添加调试日志
                        
                        if (loginData.Success && loginData.Data && loginData.Data.Token) {
                            const token = loginData.Data.Token;
                            console.log('登录成功，获取到token:', token.substring(0, 20) + '...');
                            
                            // 将token保存到localStorage，以便后续使用
                            localStorage.setItem('swagger_auth_token', token);
                            
                            // 计算token过期时间（假设token有效期为1小时）
                            const now = new Date();
                            const expiryTime = new Date(now.getTime() + 60 * 60 * 1000); // 1小时后过期
                            localStorage.setItem('swagger_auth_token_expiry', expiryTime.toISOString());
                            
                            console.log('Token过期时间:', expiryTime.toISOString());
                            console.log('当前时间:', now.toISOString());
                            
                            // 更新页面上的token显示
                            if (window.updateTokenDisplay) {
                                window.updateTokenDisplay(token);
                            }
                            
                            // 为所有请求添加Authorization头
                            if (window.ui && window.ui.getConfigs) {
                                const originalRequestInterceptor = window.ui.getConfigs().requestInterceptor;
                                
                                window.ui.getConfigs().requestInterceptor = function(request) {
                                    // 如果已有自定义请求拦截器，先调用它
                                    if (originalRequestInterceptor) {
                                        request = originalRequestInterceptor(request);
                                    }
                                    
                                    // 添加Authorization头
                                    if (!request.headers) {
                                        request.headers = {};
                                    }
                                    
                                    // 如果请求没有Authorization头，则添加Bearer token
                                    if (!request.headers.Authorization) {
                                        request.headers.Authorization = `Bearer ${token}`;
                                    }
                                    
                                    return request;
                                };
                                
                                // 刷新Swagger UI以应用新的请求拦截器
                                window.ui.specActions.download();
                            }
                        } else {
                            console.error('登录响应格式不正确:', loginData);
                        }
                    } catch (error) {
                        console.error('登录过程中发生错误:', error);
                    }
                }
                
                // 检查是否已经有token
                const existingToken = localStorage.getItem('swagger_auth_token');
                if (existingToken) {
                    console.log('使用已存在的token:', existingToken);
                    
                    // 为所有请求添加Authorization头
                    if (window.ui && window.ui.getConfigs) {
                        const originalRequestInterceptor = window.ui.getConfigs().requestInterceptor;
                        
                        window.ui.getConfigs().requestInterceptor = function(request) {
                            // 如果已有自定义请求拦截器，先调用它
                            if (originalRequestInterceptor) {
                                request = originalRequestInterceptor(request);
                            }
                            
                            // 添加Authorization头
                            if (!request.headers) {
                                request.headers = {};
                            }
                            
                            // 如果请求没有Authorization头，则添加Bearer token
                            if (!request.headers.Authorization) {
                                request.headers.Authorization = `Bearer ${existingToken}`;
                            }
                            
                            return request;
                        };
                    }
                } else {
                    // 没有token，执行登录
                    loginAndSetToken();
                }
                
                // 添加一个按钮到Swagger UI，允许用户手动刷新token
                const addAuthControls = function() {
                    // 创建容器
                    const authContainer = document.createElement('div');
                    authContainer.style.position = 'fixed';
                    authContainer.style.top = '10px';
                    authContainer.style.right = '10px';
                    authContainer.style.zIndex = '9999';
                    authContainer.style.backgroundColor = '#f8f9fa';
                    authContainer.style.border = '1px solid #dee2e6';
                    authContainer.style.borderRadius = '4px';
                    authContainer.style.padding = '10px';
                    authContainer.style.boxShadow = '0 2px 4px rgba(0,0,0,0.1)';
                    authContainer.style.minWidth = '300px';
                    
                    // 创建标题
                    const title = document.createElement('div');
                    title.textContent = '认证信息';
                    title.style.fontWeight = 'bold';
                    title.style.marginBottom = '8px';
                    title.style.color = '#495057';
                    authContainer.appendChild(title);
                    
                    // 创建token显示区域
                    const tokenDisplay = document.createElement('div');
                    tokenDisplay.style.marginBottom = '10px';
                    
                    const tokenLabel = document.createElement('div');
                    tokenLabel.textContent = 'Token:';
                    tokenLabel.style.fontSize = '12px';
                    tokenLabel.style.color = '#6c757d';
                    tokenDisplay.appendChild(tokenLabel);
                    
                    const tokenValue = document.createElement('div');
                    tokenValue.id = 'swagger-token-display';
                    tokenValue.style.fontSize = '11px';
                    tokenValue.style.color = '#212529';
                    tokenValue.style.wordBreak = 'break-all';
                    tokenValue.style.backgroundColor = '#e9ecef';
                    tokenValue.style.padding = '5px';
                    tokenValue.style.borderRadius = '3px';
                    tokenValue.style.marginTop = '3px';
                    tokenValue.style.maxHeight = '80px';
                    tokenValue.style.overflow = 'auto';
                    
                    // 显示当前token或占位符
                    const currentToken = localStorage.getItem('swagger_auth_token');
                    if (currentToken) {
                        // 只显示token的前10个字符，后面用省略号
                        const shortToken = currentToken.length > 10 ? currentToken.substring(0, 10) + '...' : currentToken;
                        tokenValue.textContent = shortToken;
                    } else {
                        tokenValue.textContent = '未获取Token';
                    }
                    
                    tokenDisplay.appendChild(tokenValue);
                        
                    // 添加过期时间显示
                    const expiryDisplay = document.createElement('div');
                    expiryDisplay.style.marginTop = '5px';
                    
                    const expiryLabel = document.createElement('div');
                    expiryLabel.textContent = '过期时间:';
                    expiryLabel.style.fontSize = '12px';
                    expiryLabel.style.color = '#6c757d';
                    expiryDisplay.appendChild(expiryLabel);
                    
                    const expiryValue = document.createElement('div');
                    expiryValue.id = 'swagger-expiry-display';
                    expiryValue.style.fontSize = '11px';
                    expiryValue.style.color = '#212529';
                    expiryValue.style.backgroundColor = '#f8f9fa';
                    expiryValue.style.padding = '5px';
                    expiryValue.style.borderRadius = '3px';
                    expiryValue.style.marginTop = '3px';
                    
                    // 显示过期时间或占位符
                    const tokenExpiry = localStorage.getItem('swagger_auth_token_expiry');
                    if (tokenExpiry) {
                        const expiryDate = new Date(tokenExpiry);
                        const now = new Date();
                        const timeLeft = Math.floor((expiryDate - now) / 1000 / 60); // 剩余分钟数
                        
                        if (timeLeft > 0) {
                            expiryValue.textContent = `${expiryDate.toLocaleString()} (剩余 ${timeLeft} 分钟)`;
                            expiryValue.style.color = '#28a745'; // 绿色表示有效
                        } else {
                            expiryValue.textContent = `${expiryDate.toLocaleString()} (已过期)`;
                            expiryValue.style.color = '#dc3545'; // 红色表示已过期
                        }
                    } else {
                        expiryValue.textContent = '未知';
                        expiryValue.style.color = '#6c757d'; // 灰色表示未知
                    }
                    
                    expiryDisplay.appendChild(expiryValue);
                    tokenDisplay.appendChild(expiryDisplay);
                    
                    authContainer.appendChild(tokenDisplay);
                    
                    // 创建刷新按钮
                    const authButton = document.createElement('button');
                    authButton.textContent = '刷新Token';
                    authButton.style.backgroundColor = '#007bff';
                    authButton.style.color = 'white';
                    authButton.style.border = 'none';
                    authButton.style.padding = '6px 12px';
                    authButton.style.borderRadius = '4px';
                    authButton.style.cursor = 'pointer';
                    authButton.style.fontSize = '12px';
                    authButton.style.marginRight = '5px';
                    
                    authButton.addEventListener('click', async function() {
                        authButton.textContent = '刷新中...';
                        authButton.disabled = true;
                        await loginAndSetToken();
                        
                        // 更新显示的token和过期时间
                        const updatedToken = localStorage.getItem('swagger_auth_token');
                        const tokenExpiry = localStorage.getItem('swagger_auth_token_expiry');
                        
                        if (updatedToken) {
                            // 只显示token的前20个字符，后面用省略号
                            const shortToken = updatedToken.length > 20 ? updatedToken.substring(0, 10) + '...' : updatedToken;
                            document.getElementById('swagger-token-display').textContent = shortToken;
                            
                            // 显示过期时间信息
                            if (tokenExpiry) {
                                const expiryDate = new Date(tokenExpiry);
                                const now = new Date();
                                const timeLeft = Math.floor((expiryDate - now) / 1000 / 60); // 剩余分钟数
                                console.log(`Token将在 ${timeLeft} 分钟后过期`);
                            }
                        } else {
                            document.getElementById('swagger-token-display').textContent = '未获取Token';
                        }
                        
                        authButton.textContent = '刷新Token';
                        authButton.disabled = false;
                    });
                    
                    authContainer.appendChild(authButton);
                    
                    // 创建复制按钮
                    const copyButton = document.createElement('button');
                    copyButton.textContent = '复制Token';
                    copyButton.style.backgroundColor = '#28a745';
                    copyButton.style.color = 'white';
                    copyButton.style.border = 'none';
                    copyButton.style.padding = '6px 12px';
                    copyButton.style.borderRadius = '4px';
                    copyButton.style.cursor = 'pointer';
                    copyButton.style.fontSize = '12px';
                    
                    copyButton.addEventListener('click', function() {
                        const token = localStorage.getItem('swagger_auth_token');
                        if (token) {
                            navigator.clipboard.writeText(token).then(function() {
                                // 临时改变按钮文本表示复制成功
                                const originalText = copyButton.textContent;
                                copyButton.textContent = '已复制!';
                                copyButton.style.backgroundColor = '#17a2b8';
                                setTimeout(function() {
                                    copyButton.textContent = originalText;
                                    copyButton.style.backgroundColor = '#28a745';
                                }, 1000);
                            }).catch(function(err) {
                                console.error('复制失败:', err);
                            });
                        }
                    });
                    
                    authContainer.appendChild(copyButton);
                    
                    // 添加到页面
                    document.body.appendChild(authContainer);
                    
                    // 更新token显示函数
                    window.updateTokenDisplay = function(token) {
                        const displayElement = document.getElementById('swagger-token-display');
                        const expiryElement = document.getElementById('swagger-expiry-display');
                        
                        if (displayElement) {
                            if (token) {
                                // 只显示token的前10个字符，后面用省略号
                                const shortToken = token.length > 10 ? token.substring(0, 10) + '...' : token;
                                displayElement.textContent = shortToken;
                            } else {
                                displayElement.textContent = '未获取Token';
                            }
                        }
                        
                        // 更新过期时间显示
                        if (expiryElement) {
                            const tokenExpiry = localStorage.getItem('swagger_auth_token_expiry');
                            if (tokenExpiry) {
                                const expiryDate = new Date(tokenExpiry);
                                const now = new Date();
                                const timeLeft = Math.floor((expiryDate - now) / 1000 / 60); // 剩余分钟数
                                
                                if (timeLeft > 0) {
                                    expiryElement.textContent = `${expiryDate.toLocaleString()} (剩余 ${timeLeft} 分钟)`;
                                    expiryElement.style.color = '#28a745'; // 绿色表示有效
                                } else {
                                    expiryElement.textContent = `${expiryDate.toLocaleString()} (已过期)`;
                                    expiryElement.style.color = '#dc3545'; // 红色表示已过期
                                }
                            } else {
                                expiryElement.textContent = '未知';
                                expiryElement.style.color = '#6c757d'; // 灰色表示未知
                            }
                        }
                    };
                };
                
                // 尝试添加认证控件
                if (document.body) {
                    addAuthControls();
                } else {
                    document.addEventListener('DOMContentLoaded', addAuthControls);
                }
                
                // 添加定时器，每分钟更新一次过期时间显示
                setInterval(function() {
                    const tokenExpiry = localStorage.getItem('swagger_auth_token_expiry');
                    const expiryElement = document.getElementById('swagger-expiry-display');
                    
                    if (tokenExpiry && expiryElement) {
                        const expiryDate = new Date(tokenExpiry);
                        const now = new Date();
                        const timeLeft = Math.floor((expiryDate - now) / 1000 / 60); // 剩余分钟数
                        
                        if (timeLeft > 0) {
                            expiryElement.textContent = `${expiryDate.toLocaleString()} (剩余 ${timeLeft} 分钟)`;
                            expiryElement.style.color = '#28a745'; // 绿色表示有效
                            
                            // 如果token将在5分钟内过期，显示警告
                            if (timeLeft <= 5) {
                                expiryElement.style.color = '#ffc107'; // 黄色表示即将过期
                                console.log(`Token将在 ${timeLeft} 分钟后过期，建议刷新`);
                            }
                        } else {
                            expiryElement.textContent = `${expiryDate.toLocaleString()} (已过期)`;
                            expiryElement.style.color = '#dc3545'; // 红色表示已过期
                            
                            // 如果token已过期，自动刷新
                            console.log('Token已过期，自动刷新');
                            loginAndSetToken();
                        }
                    }
                }, 60000); // 每分钟检查一次
                
            } catch (error) {
                console.error('初始化Swagger注入脚本时发生错误:', error);
            }
        }, 1000); // 延迟1秒确保Swagger UI完全加载
    });
})();