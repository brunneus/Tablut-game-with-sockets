﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TrabalhoSocketsUI
{
    public static class CollectionExtensions
    {
        public static void AddOnUI<T>(this ICollection<T> collection, T item)
        {
            Action<T> addMethod = collection.Add;
            Application.Current.Dispatcher.BeginInvoke(addMethod, item);

            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                //Do something here.
            }));
        }

        public static void ClearOnUI<T>(this ICollection<T> collection)
        {
            Application.Current.Dispatcher.Invoke((() =>
            {
                collection.Clear();
            }));
        }

    }
}
