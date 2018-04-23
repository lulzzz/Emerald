using System;
using System.Collections.Generic;
using System.Linq;

namespace Emerald.Utils
{
    public static class CollectionHelper
    {
        public static List<TCollectionItem> Update<TCollectionItem, TUpdatePackItem>(
            List<TCollectionItem> collection,
            TUpdatePackItem[] updatePack,
            Func<TCollectionItem, TUpdatePackItem, bool> equalsFunc,
            Func<TUpdatePackItem, TCollectionItem> createFunc,
            Action<TCollectionItem, TUpdatePackItem> updateAction,
            out TCollectionItem[] inserted,
            out TCollectionItem[] updated,
            out TCollectionItem[] deleted)
        {
            inserted = new TCollectionItem[0];
            updated = new TCollectionItem[0];
            deleted = new TCollectionItem[0];

            if (updatePack == null || updatePack.Length == 0)
            {
                deleted = collection?.ToArray() ?? deleted;
                collection?.Clear();
                return collection;
            }

            collection = collection ?? new List<TCollectionItem>(updatePack.Length);

            if (collection.Count > 0)
            {
                deleted = collection.Where(ci => !updatePack.Any(upi => equalsFunc(ci, upi))).ToArray();
                foreach (var item in deleted) collection.Remove(item);
            }

            var insertedList = new List<TCollectionItem>();
            var updatedList = new List<TCollectionItem>();

            foreach (var updatePackItem in updatePack)
            {
                var collectionItem = collection.SingleOrDefault(ci => equalsFunc(ci, updatePackItem));

                if (collectionItem == null)
                {
                    collectionItem = createFunc(updatePackItem);
                    collection.Add(collectionItem);
                    insertedList.Add(collectionItem);
                }
                else
                {
                    updateAction(collectionItem, updatePackItem);
                    updatedList.Add(collectionItem);
                }
            }

            inserted = insertedList.ToArray();
            updated = updatedList.ToArray();

            return collection;
        }

        public static List<TCollectionItem> Update<TCollectionItem, TUpdatePackItem>(
            List<TCollectionItem> collection,
            TUpdatePackItem[] updatePack,
            Func<TCollectionItem, TUpdatePackItem, bool> equalsFunc,
            Func<TUpdatePackItem, TCollectionItem> createFunc,
            Action<TCollectionItem, TUpdatePackItem> updateAction)
        {
            return Update(collection, updatePack, equalsFunc, createFunc, updateAction, out var _, out var _, out var _);
        }
    }
}