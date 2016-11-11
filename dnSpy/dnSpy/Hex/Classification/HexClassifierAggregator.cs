﻿/*
    Copyright (C) 2014-2016 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using dnSpy.Contracts.Hex;
using dnSpy.Contracts.Hex.Classification;
using dnSpy.Contracts.Hex.Tagging;
using Microsoft.VisualStudio.Text.Classification;

namespace dnSpy.Hex.Classification {
	abstract class HexClassifierAggregator : HexClassifier {
		readonly IClassificationTypeRegistryService classificationTypeRegistryService;
		readonly HexTagAggregator<HexClassificationTag> hexTagAggregator;
		readonly HexBuffer hexBuffer;

		public override event EventHandler<HexClassificationChangedEventArgs> ClassificationChanged;

		protected HexClassifierAggregator(HexTagAggregator<HexClassificationTag> hexTagAggregator, IClassificationTypeRegistryService classificationTypeRegistryService, HexBuffer hexBuffer) {
			if (hexTagAggregator == null)
				throw new ArgumentNullException(nameof(hexTagAggregator));
			if (classificationTypeRegistryService == null)
				throw new ArgumentNullException(nameof(classificationTypeRegistryService));
			if (hexBuffer == null)
				throw new ArgumentNullException(nameof(hexBuffer));
			this.classificationTypeRegistryService = classificationTypeRegistryService;
			this.hexTagAggregator = hexTagAggregator;
			this.hexBuffer = hexBuffer;
			hexTagAggregator.TagsChanged += HexTagAggregator_TagsChanged;
		}

		void HexTagAggregator_TagsChanged(object sender, HexTagsChangedEventArgs e) =>
			ClassificationChanged?.Invoke(this, new HexClassificationChangedEventArgs(e.Span));

		sealed class HexClassificationSpanComparer : IComparer<HexClassificationSpan> {
			public static readonly HexClassificationSpanComparer Instance = new HexClassificationSpanComparer();
			public int Compare(HexClassificationSpan x, HexClassificationSpan y) => x.Span.Start.Position.CompareTo(y.Span.Start.Position);
		}

		public override void GetClassificationSpans(List<HexClassificationSpan> result, HexClassificationContext context) =>
			GetClassificationSpansCore(result, context, null);

		public override void GetClassificationSpans(List<HexClassificationSpan> result, HexClassificationContext context, CancellationToken cancellationToken) =>
			GetClassificationSpansCore(result, context, cancellationToken);

		void GetClassificationSpansCore(List<HexClassificationSpan> result, HexClassificationContext context, CancellationToken? cancellationToken) {
			var span = context.LineSpan;
			if (span.Length == 0)
				return;

			var list = new List<HexClassificationSpan>();
			var tags = cancellationToken != null ? hexTagAggregator.GetTags(span, cancellationToken.Value) : hexTagAggregator.GetTags(span);
			foreach (var mspan in tags) {
				var overlap = span.Overlap(mspan.Span);
				if (overlap != null)
					list.Add(new HexClassificationSpan(overlap.Value, mspan.Tag.ClassificationType));
			}

			if (list.Count <= 1) {
				if (result.Count == 1)
					result.Add(result[0]);
				return;
			}

			list.Sort(HexClassificationSpanComparer.Instance);

			// Common case
			if (!HasOverlaps(list)) {
				result.AddRange(Merge(list));
				return;
			}

			int min = 0;
			var minOffset = span.Start.Position;
			var newList = new List<HexClassificationSpan>();
			var ctList = new List<IClassificationType>();
			while (min < list.Count) {
				while (min < list.Count && minOffset >= list[min].Span.End)
					min++;
				if (min >= list.Count)
					break;
				var cspan = list[min];
				minOffset = HexPosition.Max(minOffset, cspan.Span.Start.Position);
				var end = cspan.Span.End.Position;
				ctList.Clear();
				ctList.Add(cspan.ClassificationType);
				for (int i = min + 1; i < list.Count; i++) {
					cspan = list[i];
					var cspanStart = cspan.Span.Start.Position;
					if (cspanStart > minOffset) {
						if (cspanStart < end)
							end = cspanStart;
						break;
					}
					var cspanEnd = cspan.Span.End.Position;
					if (minOffset >= cspanEnd)
						continue;
					if (cspanEnd < end)
						end = cspanEnd;
					if (!ctList.Contains(cspan.ClassificationType))
						ctList.Add(cspan.ClassificationType);
				}
				Debug.Assert(minOffset < end);
				var newSnapshotSpan = new HexBufferSpan(span.Buffer, HexSpan.FromBounds(minOffset, end));
				var ct = ctList.Count == 1 ? ctList[0] : classificationTypeRegistryService.CreateTransientClassificationType(ctList);
				newList.Add(new HexClassificationSpan(newSnapshotSpan, ct));
				minOffset = end;
			}

			Debug.Assert(!HasOverlaps(newList));
			result.AddRange(Merge(newList));
			return;
		}

		static List<HexClassificationSpan> Merge(List<HexClassificationSpan> list) {
			if (list.Count <= 1)
				return list;

			var prev = list[0];
			int read = 1, write = 0;
			for (; read < list.Count; read++) {
				var a = list[read];
				if (prev.ClassificationType == a.ClassificationType && prev.Span.End == a.Span.Start)
					list[write] = prev = new HexClassificationSpan(HexBufferSpan.FromBounds(prev.Span.Start, a.Span.End), prev.ClassificationType);
				else {
					prev = a;
					list[++write] = a;
				}
			}
			write++;
			if (list.Count != write)
				list.RemoveRange(write, list.Count - write);

			return list;
		}

		static bool HasOverlaps(List<HexClassificationSpan> sortedList) {
			for (int i = 1; i < sortedList.Count; i++) {
				if (sortedList[i - 1].Span.End > sortedList[i].Span.Start)
					return true;
			}
			return false;
		}

		protected override void DisposeCore() {
			hexTagAggregator.TagsChanged -= HexTagAggregator_TagsChanged;
			hexTagAggregator.Dispose();
		}
	}
}
