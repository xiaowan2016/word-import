﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Aspose.Words;
using Aspose.Words.Drawing;
using Common.Logging;
using Tunnel.Word.Chart;
using Tunnel.Word.Enums;
using Tunnel.Word.Monitoring;
using Tunnel.Word.Table;

namespace Tunnel.Word.BuildWord.MonitorMonth
{
    public class TunnelMonitorService
    {
        private WordManage _word;
        private readonly string _basePath;
        private CoverModel _model;
        private BodyModel _bodyModel;
        private List<EnclosureModel> _enclosureModels;
        private string _path;
        private int _coverCount = 0;
        private readonly int _spaceLength=27;

        /// <summary>
        /// 表格的索引 表1 表2....
        /// </summary>
        private int _tableIndex = 1;
        /// <summary>
        /// 图的索引 图1 图2....
        /// </summary>
        private int _PicIndex = 1;
        private readonly ILog _logger = LogManager.GetCurrentClassLogger();
        public TunnelMonitorService()
        {
            _basePath = Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                "TunnelMonitorFiles");
        }

        public Document BuildWord()
        {
            _word = new WordManage();
            _word.Open(GetSroce("Tunnel.Word.TunnelMonitorFiles.封面.docx"));

            BuildCover();
            AddBodyAndSetData();
            return _word.Document;
        }

        /// <summary>
        /// 设置封面模型
        /// </summary>
        /// <param name="model"></param>
        public void SetCoverModel(CoverModel model)
        {
            this._model = model;
        }

        /// <summary>
        /// 设置附件数据
        /// </summary>
        /// <param name="models"></param>
        public void SetEnclosureModel(List<EnclosureModel> models)
        {
            this._enclosureModels = models;
        }

        /// <summary>
        /// 设置主体数据
        /// </summary>
        /// <param name="model"></param>
        public void SetBodyModel(BodyModel model)
        {
            this._bodyModel = model;
        }

        /// <summary>
        /// 生成封面
        /// </summary>
        private void BuildCover()
        {
            if (this._model == null) return;
            WordManage.SetModel(this._model, _word.Document, _word.Builder);
        }

        private void AddBodyAndSetData()
        {
            var jklcDocument = new Aspose.Words.Document(GetSroce("Tunnel.Word.TunnelMonitorFiles.监控量测.docx"));
            var builder = new DocumentBuilder(jklcDocument);
            if (this._bodyModel != null)
                WordManage.SetModel(this._bodyModel, jklcDocument, builder);

            jklcDocument.UpdateFields();
            _coverCount = jklcDocument.PageCount - 2;
            _word.Document.AppendDocument(jklcDocument, ImportFormatMode.KeepSourceFormatting);

            SetEnclosure();

            builder.MoveToHeaderFooter(HeaderFooterType.FooterPrimary);
            WordManage.BookMarkReplace(_word.Document, builder, "totalPage", _coverCount.ToString());

        }

        /// <summary>
        /// 处理附件
        /// </summary>
        private void SetEnclosure()
        {
            if (_enclosureModels == null) return;
            foreach (var enclosureModel in _enclosureModels)
            {
                if (enclosureModel.Type == EnclosureType.Table)
                {
                    SetEnclosure1(enclosureModel);
                }
                else if (enclosureModel.Type == EnclosureType.Image)
                {
                    SetEnclosure2(enclosureModel);
                }
            }
        }

        private void SetEnclosure1(EnclosureModel model)
        {
            var enclosurelDocument = new Aspose.Words.Document(GetSroce("Tunnel.Word.TunnelMonitorFiles.附件1.docx"));
            var builder = new DocumentBuilder(enclosurelDocument);
            WordManage.SetModel(model, enclosurelDocument, builder);
            _word.Document.AppendDocument(enclosurelDocument, ImportFormatMode.UseDestinationStyles);

            _word.Document.UpdateFields();

            _coverCount += enclosurelDocument.PageCount;
        }
        private void SetEnclosure2(EnclosureModel model)
        {
            var enclosurelDocument = new Aspose.Words.Document(GetSroce("Tunnel.Word.TunnelMonitorFiles.附件2.docx"));
            var builder = new DocumentBuilder(enclosurelDocument);
            WordManage.SetModel(model, enclosurelDocument, builder);

            int images = model.Images == null ? 0 : model.Images.Count;

            for (int i = 0; i < 8; i++)
            {
                if (model.Images != null && (i < images && model.Images[i] != null))
                {
                    var bm = enclosurelDocument.Range.Bookmarks["Image" + (i + 1)];
                    if (bm != null)
                    {
                        builder.MoveToBookmark("Image" + (i + 1));
                        bm.Text = "";
                        Shape shape = builder.InsertImage(model.Images[i].ImageUrl);
                        shape.Width = 195;
                    }

                    WordManage.BookMarkReplace(enclosurelDocument, builder, "Image" + (i + 1) + "_Name", model.Images[i].ImageName);
                }
                else
                {
                    WordManage.BookMarkReplace(enclosurelDocument, builder, "Image" + (i + 1), "");
                    WordManage.BookMarkReplace(enclosurelDocument, builder, "Image" + (i + 1) + "_Name", "");
                }

            }


            _word.Document.AppendDocument(enclosurelDocument, ImportFormatMode.UseDestinationStyles);

            _word.Document.UpdateFields();

            _coverCount += enclosurelDocument.PageCount;
        }

        private Stream GetSroce(string url)
        {
            Assembly assm = Assembly.GetExecutingAssembly();
            return assm.GetManifestResourceStream(url);
        }
   
    }
}
