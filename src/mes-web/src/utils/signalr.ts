import * as signalR from '@microsoft/signalr'

type EventHandler = (...args: any[]) => void

class SignalRService {
  private connection: signalR.HubConnection | null = null
  private handlers: Map<string, EventHandler[]> = new Map()
  private isConnected = false
  private reconnectTimer: number | null = null

  /**
   * 建立 SignalR 连接
   */
  async connect(url: string = '/hubs/mes'): Promise<void> {
    if (this.connection && this.isConnected) return

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(url)
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build()

    // 注册所有已有的事件处理器
    this.handlers.forEach((handlerList, event) => {
      handlerList.forEach(h => {
        this.connection?.on(event, h)
      })
    })

    // 重连状态监听
    this.connection.onreconnecting(() => {
      this.isConnected = false
    })

    this.connection.onreconnected(() => {
      this.isConnected = true
      // 重新加入 dashboard 组
      this.joinGroup('dashboard')
    })

    this.connection.onclose(() => {
      this.isConnected = false
    })

    try {
      await this.connection.start()
      this.isConnected = true
      // 默认加入 dashboard 组
      await this.joinGroup('dashboard')
    } catch (err) {
      console.error('SignalR 连接失败:', err)
      this.isConnected = false
      // 尝试重连
      this.scheduleReconnect(url)
    }
  }

  /**
   * 断开连接
   */
  async disconnect(): Promise<void> {
    if (this.reconnectTimer) {
      clearTimeout(this.reconnectTimer)
      this.reconnectTimer = null
    }
    if (this.connection) {
      await this.connection.stop()
      this.isConnected = false
    }
  }

  /**
   * 加入组
   */
  async joinGroup(groupName: string): Promise<void> {
    if (this.connection && this.isConnected) {
      try {
        await this.connection.invoke('JoinGroup', groupName)
      } catch (err) {
        console.error(`加入组 ${groupName} 失败:`, err)
      }
    }
  }

  /**
   * 离开组
   */
  async leaveGroup(groupName: string): Promise<void> {
    if (this.connection && this.isConnected) {
      try {
        await this.connection.invoke('LeaveGroup', groupName)
      } catch (err) {
        console.error(`离开组 ${groupName} 失败:`, err)
      }
    }
  }

  /**
   * 订阅事件
   */
  on(event: string, handler: EventHandler): void {
    const list = this.handlers.get(event) || []
    list.push(handler)
    this.handlers.set(event, list)

    // 如果已连接，立即注册
    if (this.connection) {
      this.connection.on(event, handler)
    }
  }

  /**
   * 取消订阅事件
   */
  off(event: string, handler?: EventHandler): void {
    if (handler) {
      const list = this.handlers.get(event) || []
      const idx = list.indexOf(handler)
      if (idx >= 0) list.splice(idx, 1)
      if (this.connection) {
        this.connection.off(event, handler)
      }
    } else {
      this.handlers.delete(event)
      if (this.connection) {
        this.connection.off(event)
      }
    }
  }

  /**
   * 连接状态
   */
  get connected(): boolean {
    return this.isConnected
  }

  private scheduleReconnect(url: string): void {
    this.reconnectTimer = window.setTimeout(() => {
      this.connect(url)
    }, 5000)
  }
}

// 单例导出
const signalRService = new SignalRService()
export default signalRService

// 便捷的事件订阅导出
export const onOrderUpdate = (handler: EventHandler) => signalRService.on('onOrderUpdate', handler)
export const onOutputUpdate = (handler: EventHandler) => signalRService.on('onOutputUpdate', handler)
export const onAndonEvent = (handler: EventHandler) => signalRService.on('onAndonEvent', handler)
export const onOeeUpdate = (handler: EventHandler) => signalRService.on('onOeeUpdate', handler)
