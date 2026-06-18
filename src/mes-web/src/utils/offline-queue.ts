import { openDB, type IDBPDatabase } from 'idb'

interface QueueItem {
  id?: number
  type: string
  payload: any
  createdAt: string
  retryCount: number
}

const DB_NAME = 'mes-offline-queue'
const STORE_NAME = 'queue'

let dbPromise: Promise<IDBPDatabase> | null = null

function getDb(): Promise<IDBPDatabase> {
  if (!dbPromise) {
    dbPromise = openDB(DB_NAME, 1, {
      upgrade(db) {
        if (!db.objectStoreNames.contains(STORE_NAME)) {
          db.createObjectStore(STORE_NAME, { keyPath: 'id', autoIncrement: true })
        }
      }
    })
  }
  return dbPromise
}

export const offlineQueue = {
  async add(type: string, payload: any) {
    const db = await getDb()
    await db.add(STORE_NAME, { type, payload, createdAt: new Date().toISOString(), retryCount: 0 })
  },

  async getAll(): Promise<QueueItem[]> {
    const db = await getDb()
    return db.getAll(STORE_NAME)
  },

  async remove(id: number) {
    const db = await getDb()
    await db.delete(STORE_NAME, id)
  },

  async incrementRetry(id: number) {
    const db = await getDb()
    const item = await db.get(STORE_NAME, id)
    if (item) {
      item.retryCount++
      await db.put(STORE_NAME, item)
    }
  },

  async clear() {
    const db = await getDb()
    await db.clear(STORE_NAME)
  },

  async count(): Promise<number> {
    const db = await getDb()
    return db.count(STORE_NAME)
  }
}
