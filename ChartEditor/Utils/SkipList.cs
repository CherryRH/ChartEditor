using ChartEditor.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartEditor.Utils
{
    /// <summary>
    /// 跳表
    /// </summary>
    public class SkipList<TKey, TValue>
    {
        private readonly Random random = new Random();

        private readonly SkipListNode<TKey, TValue> head;
        public SkipListNode<TKey, TValue> Head { get { return head; } }

        private SkipListNode<TKey, TValue> lastNode;
        public SkipListNode<TKey, TValue> LastNode { get { return lastNode; } }

        private readonly Comparison<TKey> comparer;

        private readonly int maxLevel;

        private int level;

        public int Count { get; set; } = 0;

        /// <summary>
        /// 第一个节点
        /// </summary>
        public SkipListNode<TKey, TValue> FirstNode { get { return head.Next[0]; } }

        public SkipList(int maxLevel, Comparison<TKey> comparer)
        {
            this.maxLevel = maxLevel;
            this.level = 1;
            this.comparer = comparer;
            this.head = new SkipListNode<TKey, TValue>(default, default, this.maxLevel);
            this.lastNode = this.head; 
        }

        /// <summary>
        /// 获取一个随机高度
        /// </summary>
        private int GetRandomLevel()
        {
            int level = 1;
            while (random.Next(0, 2) == 0 && level < maxLevel)
            {
                level++;
            }
            return level;
        }

        /// <summary>
        /// 尝试获取值，不存在则返回false
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            SkipListNode<TKey, TValue> current = head;

            for (int i = level - 1; i >= 0; i--)
            {
                while (current.Next[i] != null && comparer(current.Next[i].Key, key) < 0)
                {
                    current = current.Next[i];
                }

                if (current.Next[i] != null && comparer(current.Next[i].Key, key) == 0)
                {
                    value = current.Next[i].Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// 尝试获取节点
        /// </summary>
        public bool TryGetNode(TKey key, out SkipListNode<TKey, TValue> skipListNode)
        {
            SkipListNode<TKey, TValue> current = head;

            for (int i = level - 1; i >= 0; i--)
            {
                while (current.Next[i] != null && comparer(current.Next[i].Key, key) < 0)
                {
                    current = current.Next[i];
                }

                if (current.Next[i] != null && comparer(current.Next[i].Key, key) == 0)
                {
                    skipListNode = current.Next[i];
                    return true;
                }
            }

            skipListNode = default;
            return false;
        }

        /// <summary>
        /// 尝试获取节点或下一个节点
        /// </summary>
        public SkipListNode<TKey, TValue> TryGetNodeOrNext(TKey key)
        {
            SkipListNode<TKey, TValue> current = head;

            for (int i = level - 1; i >= 0; i--)
            {
                while (current.Next[i] != null && comparer(current.Next[i].Key, key) < 0)
                {
                    current = current.Next[i];
                }
            }
            return current.Next[0];
        }

        /// <summary>
        /// 插入一个元素
        /// </summary>
        public bool Insert(TKey key, TValue value)
        {
            var update = new SkipListNode<TKey, TValue>[maxLevel];
            SkipListNode<TKey, TValue> current = head;

            for (int i = level - 1; i >= 0; i--)
            {
                while (current.Next[i] != null && comparer(current.Next[i].Key, key) < 0)
                {
                    current = current.Next[i];
                }

                if (current.Next[i] != null && comparer(current.Next[i].Key, key) == 0)
                {
                    return false;
                }

                update[i] = current;
            }

            int newLevel = GetRandomLevel();
            if (newLevel > level)
            {
                for (int i = level; i < newLevel; i++)
                {
                    update[i] = head;
                }
                level = newLevel;
            }

            SkipListNode<TKey, TValue> newNode = new SkipListNode<TKey, TValue>(key, value, newLevel);
            for (int i = 0; i < newLevel; i++)
            {
                newNode.Next[i] = update[i].Next[i];
                update[i].Next[i] = newNode;
            }
            if (lastNode == head || comparer(lastNode.Key, key) < 0)
            {
                lastNode = newNode;
            }
            this.Count++;
            return true;
        }

        /// <summary>
        /// 移除一个元素
        /// </summary>
        public bool Delete(TKey key)
        {
            var update = new SkipListNode<TKey, TValue>[maxLevel];
            SkipListNode<TKey, TValue> current = head;

            for (int i = level - 1; i >= 0; i--)
            {
                while (current.Next[i] != null && comparer(current.Next[i].Key, key) < 0)
                {
                    current = current.Next[i];
                }

                update[i] = current;
            }

            current = current.Next[0];
            if (current != null && comparer(current.Key, key) == 0)
            {
                for (int i = 0; i < level; i++)
                {
                    if (update[i].Next[i] != current) break;
                    update[i].Next[i] = current.Next[i];
                }

                while (level > 1 && head.Next[level - 1] == null)
                {
                    level--;
                }

                if (comparer(lastNode.Key, key) == 0)
                {
                    lastNode = head;
                    for (int i = level - 1; i >= 0; i--)
                    {
                        while (lastNode.Next[i] != null)
                        {
                            lastNode = lastNode.Next[i];
                        }
                    }
                }
                this.Count--;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 找到目标的前项
        /// </summary>
        public SkipListNode<TKey, TValue> GetPreNode(TKey target)
        {
            SkipListNode<TKey, TValue> current = head;

            for (int i = level - 1; i >= 0; i--)
            {
                while (current.Next[i] != null && comparer(current.Next[i].Key, target) < 0)
                {
                    current = current.Next[i];
                }
            }

            return current;
        }
    }

    public class SkipListNode<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; } = default(TValue);

        public SkipListNode<TKey, TValue>[] Next;

        public SkipListNode(TKey key, TValue value, int level)
        {
            this.Key = key;
            this.Value = value;
            this.Next = new SkipListNode<TKey, TValue>[level];
        }
    }
}
