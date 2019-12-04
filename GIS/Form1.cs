using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Output;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Analyst3D;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GlobeCore;


namespace GIS
{
    public partial class Form1 : Form
    {
        ILayer pMoveLayer;//图层全局变量
        int toIndex;
        public Form1()
        {
            //获取权限
            ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.EngineOrDesktop);
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //关联图层与地图
            axTOCControl2.SetBuddyControl(axMapControl1);
            this.defaultLookAndFeel1.LookAndFeel.SkinName = "DevExpress  Dark Style";
        }
        bool m_blSelect = false;
        //打开地图按钮
        private void Btn_Open_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Btn_Down_Control(Btn_Open);
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = "C:\\DISK_D\\myprojects\\data\\";
            ofd.Filter = "Map Document|*.mxd";
            ofd.RestoreDirectory = true;
            ofd.FilterIndex = 1;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                IMapControlDefault pMCD;
                pMCD = axMapControl1.Object as IMapControlDefault;
                string strFileName = ofd.FileName;
                bool bRet = pMCD.CheckMxFile(strFileName);
                if (bRet)
                {
                    pMCD.LoadMxFile(strFileName, null, Type.Missing);
                    //打开鹰眼图
                    axMapControl2.LoadMxFile(strFileName);
                    ControlsMapFullExtentCommand FullMap = new ControlsMapFullExtentCommand();
                    FullMap.OnCreate(axMapControl2.Object);
                    FullMap.OnClick();
                }

            }
            //获取图层名称到查询下拉菜单
            this.Cbx_Query.Properties.Items.Clear();
            repositoryItemComboBox3.Properties.Items.Clear();
            for (int i = 0; i < axMapControl1.LayerCount; i++)
            {
                string tmp_strName = axMapControl1.get_Layer(i).Name;
                this.Cbx_Query.Properties.Items.AddRange(new object[] { tmp_strName });
                repositoryItemComboBox3.Properties.Items.AddRange(new object[] { tmp_strName });
            }
            this.Cbx_Query.Text = axMapControl1.get_Layer(0).Name;                     
        }

        //放大按钮
        private void Btn_Zoomin_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Btn_Down_Control(Btn_Zoomin);
            ICommand Cmd = new ControlsMapZoomInTool();
            Cmd.OnCreate(this.axMapControl1.Object);
            this.axMapControl1.CurrentTool = Cmd as ITool;                     
        }
        //缩小按钮
        private void Btn_Zoomout_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Btn_Down_Control(Btn_Zoomout);
            ICommand Cmd = new ControlsMapZoomOutTool();
            Cmd.OnCreate(this.axMapControl1.Object);
            this.axMapControl1.CurrentTool = Cmd as ITool;
            
        }
        //漫游按钮
        private void Btn_Pan_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Btn_Down_Control(Btn_Pan);
            axMapControl1.CurrentTool = null;
            ICommand Cmd = new ControlsMapPanTool();
            Cmd.OnCreate(this.axMapControl1.Object);
            this.axMapControl1.CurrentTool = Cmd as ITool;
            m_blSelect = false;
        }
        //属性工具按钮
        private void Btn_Feature_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Btn_Down_Control(Btn_Feature);
            ICommand Cmd = new ControlsMapIdentifyTool();
            Cmd.OnCreate(this.axMapControl1.Object);
            this.axMapControl1.CurrentTool = Cmd as ITool;          
        }
        //测量工具按钮
        private void Btn_Meature_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Btn_Down_Control(Btn_Meature);
            ICommand Cmd = new ControlsMapMeasureTool();
            Cmd.OnCreate(this.axMapControl1.Object);
            this.axMapControl1.CurrentTool = Cmd as ITool;     
        }
        //选择要素按钮
        private void Btn_Select_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {          
            Btn_Down_Control(Btn_Select);
            m_blSelect = false;
            ICommand Cmd = new ControlsSelectFeaturesTool();
            Cmd.OnCreate(this.axMapControl1.Object);
            this.axMapControl1.CurrentTool = Cmd as ITool;        
        }
        
        private void axMapControl1_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            if (m_blSelect == true)
            {
                //多边形选择
                IMapControlDefault pMCD;
                pMCD = axMapControl1.Object as IMapControlDefault;
                IMap pMap;
                pMap = pMCD.Map;

                IGeometry pGeom;
                pGeom = pMCD.TrackPolygon();
                pMap.SelectByShape(pGeom, null, false);

                pMCD.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                m_blSelect = false;
            }
            if (axMapControl1.MousePointer == esriControlsMousePointer.esriPointerCrosshair)
            {
                try
                {
                    //获取图层名
                    string StrBZLyrName = Cbx_Lay.EditValue.ToString();
                    //添加点坐标
                    IPoint pntAdd = new ESRI.ArcGIS.Geometry.Point();
                    pntAdd.X = e.mapX;
                    pntAdd.Y = e.mapY;

                    //获取查询图层ID
                    int iLyr = 0;
                    for (int i = 0; i < axMapControl1.LayerCount; i++)
                    {
                        if (axMapControl1.get_Layer(i).Name == StrBZLyrName)
                        {
                            iLyr = i;
                            break;
                        }
                    }

                    //得到要添加地物的图层
                    IFeatureLayer l = axMapControl1.Map.get_Layer(iLyr) as IFeatureLayer;

                    //获取要素图层的要素类对象
                    IFeatureClass fc = l.FeatureClass;

                    //定义一个编辑的工作空间              
                    IWorkspaceEdit w = (fc as IDataset).Workspace as IWorkspaceEdit;
                    IFeatureBuffer f = fc.CreateFeatureBuffer();
                    //定义一个插入的要素Cursor                
                    IFeatureCursor cur = fc.Insert(true);

                    //开始事务操作
                    w.StartEditing(false);
                    //开始编辑
                    w.StartEditOperation();

                    //创建一个地物
                    f.Shape = pntAdd;
                    //插入地物
                    cur.InsertFeature(f);

                    //结束编辑
                    w.StopEditOperation();
                    //结束事务操作
                    w.StopEditing(true);

                    //刷新地图
                    IActiveView pActiveView = axMapControl1.Map as IActiveView;
                    pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, axMapControl1.Map.get_Layer(iLyr), null);
                }
                catch
                {
                    MessageBox.Show("请选择图层");
                }
            }
            else  if (axMapControl1.MousePointer == esriControlsMousePointer.esriPointerHotLink)
            {
//获取预标记图层名称
                try
                {
                    string StrBZLyrName = Cbx_Lay.EditValue.ToString();
                    //获取鼠标点击的地图坐标
                    IPoint pntDel = new ESRI.ArcGIS.Geometry.Point();
                    pntDel.X = e.mapX;
                    pntDel.Y = e.mapY;
                    //获取编辑图层索引号
                    int iLyr = 0;
                    for (int i = 0; i < axMapControl1.LayerCount; i++)
                    {
                        if (axMapControl1.get_Layer(i).Name == StrBZLyrName)
                        {
                            iLyr = i;
                            break;
                        }
                    }
                   //获取图层FeatureLayer对象
                    IFeatureLayer layer = axMapControl1.Map.get_Layer(iLyr) as IFeatureLayer;

                    //FindFeature为寻找空间要素自定义函数
                    IFeature feature = FindFeature(layer, pntDel);
                    if (feature != null)
                    {
                        //获取预删除对象FID
                        string fFID = feature.get_Value(0).ToString();
                        IFeatureClass fc = layer.FeatureClass;
                        IQueryFilter pQueryFilter = new QueryFilterClass();
                        //查询条件为空表示删除所有点
                        pQueryFilter.WhereClause = "fid=" + fFID;
                        ITable pTable = fc as ITable;

                        IWorkspaceEdit w = (fc as IDataset).Workspace as IWorkspaceEdit;
                        //开始事务操作
                        w.StartEditing(false);
                        //开始编辑
                        w.StartEditOperation();
                        pTable.DeleteSearchedRows(pQueryFilter);
                        //结束编辑
                        w.StopEditOperation();
                        //结束事务操作
                        w.StopEditing(true);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pQueryFilter);

                        IActiveView pActiveView = axMapControl1.Map as IActiveView;
                        pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, axMapControl1.Map.get_Layer(iLyr), null);
                    }
                }
                catch
                {
                    MessageBox.Show("请选择图层");
                }
            }
            else if (axMapControl1.MousePointer == esriControlsMousePointer.esriPointerIdentify)
            {
                try
                {
                    //获取预标记图层名称
                    string StrBZLyrName = Cbx_Lay.EditValue.ToString();

                    //获取鼠标点击的地图坐标
                    IPoint pntEdit = new ESRI.ArcGIS.Geometry.Point();
                    pntEdit.X = e.mapX;
                    pntEdit.Y = e.mapY;

                    //获取编辑图层索引号
                    int iLyr = 0;
                    for (int i = 0; i < axMapControl1.LayerCount; i++)
                    {
                        if (axMapControl1.get_Layer(i).Name == StrBZLyrName)
                        {
                            iLyr = i;
                            break;
                        }
                    }
                    //编辑DockPanel的标题设置为标记图层名
                    Info_Dock.Text = StrBZLyrName;
                    //获取图层FeatureLayer对象
                    IFeatureLayer layer = axMapControl1.Map.get_Layer(iLyr) as IFeatureLayer;

                    //FindFeature为寻找空间要素自定义函数，上周已经写过这个函数
                    IFeature feature = FindFeature(layer, pntEdit);
                    if (feature != null)
                    {
                        //ID标签name属性为Fid，获取编辑要素Fid值
                        Label_Edit_ID.Text = feature.get_Value(0).ToString();
                        //获取编辑要素"名称"字段索引值

                        int IntName = feature.Fields.FindField("名称");
                        //名称标签EditName属性为Name
                        Txb_Edit_Name.Text = feature.get_Value(IntName).ToString();
                        //获取编辑要素"类型"字段索引值
                        int IntCata = feature.Fields.FindField("类别");
                        //类型标签name属性为Cata
                        Cbx_Edit_Class.Text = feature.get_Value(IntCata).ToString();
                        //获取编辑要素"备注"字段索引值
                        int IntRemark = feature.Fields.FindField("备注");
                        //备注标签name属性为Remark
                        Txb_Edit_Remark.Text = feature.get_Value(IntRemark).ToString();
                        Info_Dock.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
                    }
                }

                catch
                {
                    MessageBox.Show("请选择图层");
                }

            }
        
        }
        //多边形选择逻辑变量修改
        private void Btn_TarckPolygon_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            axMapControl1.CurrentTool = null; 
            Btn_Down_Control(Btn_TarckPolygon);
            m_blSelect = true;
            
        }
        //鹰眼图
        private void axMapControl2_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            IPoint Pnt;
            Pnt = new ESRI.ArcGIS.Geometry.Point();
            Pnt.PutCoords(e.mapX, e.mapY);
            axMapControl1.CenterAt(Pnt);
            axMapControl1.Refresh();
        }

        //鹰眼图与主图交互
        private void axMapControl1_OnExtentUpdated(object sender, IMapControlEvents2_OnExtentUpdatedEvent e)
        {
            //定义边界对象

            try
            {
                IEnvelope pEnv;
                pEnv = e.newEnvelope as IEnvelope;
                IGraphicsContainer pGraphicsContainer;
                IActiveView pActiveView;
                //获取鹰眼图地图数据的图形容器句柄
                pGraphicsContainer = axMapControl2.Map as IGraphicsContainer;
                pActiveView = pGraphicsContainer as IActiveView;
                pGraphicsContainer.DeleteAllElements();
                RectangleElement pRectangleEle;

                pRectangleEle = new RectangleElement();

                IElement pEle;
                pEle = pRectangleEle as IElement;
                pEle.Geometry = pEnv;

                IRgbColor pColor;
                pColor = new RgbColor();
                pColor.Transparency = 255;

                pColor.Red = 255;
                pColor.Blue = 0;
                pColor.Green = 0;
                ILineSymbol pOutline;
                pOutline = new SimpleLineSymbol();
                pOutline.Width = 1;
                pOutline.Color = pColor;
                pColor = new RgbColor();
                pColor.Transparency = 0;

                IFillSymbol pFillSymbol;
                pFillSymbol = new SimpleFillSymbol();
                pFillSymbol.Color = pColor;
                pFillSymbol.Outline = pOutline;
                IFillShapeElement pFillshapeEle;
                pFillshapeEle = pEle as IFillShapeElement;
                pFillshapeEle.Symbol = pFillSymbol;
                pEle = pFillshapeEle as IElement;
                pGraphicsContainer.AddElement(pEle, 0);
                pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            }
            catch
            {

            }
        }
        //图层控制
        private void axTOCControl2_OnMouseDown(object sender, ITOCControlEvents_OnMouseDownEvent e)
        {
            if (e.button == 1)
            {
                esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
                IBasicMap map = null;
                ILayer layer = null;
                object other = null;
                object index = null;
                axTOCControl2.HitTest(e.x, e.y, ref item, ref map, ref layer, ref other, ref index);
                if (item == esriTOCControlItem.esriTOCControlItemLayer)
                {
                    if (layer is IAnnotationSublayer)   //注记层在表层，不参与移动
                        return;
                    else
                        pMoveLayer = layer;
                }
            }
        }

        private void axTOCControl2_OnMouseUp(object sender, ITOCControlEvents_OnMouseUpEvent e)
        {
            if (e.button == 1)
            {
                esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
                IBasicMap map = null;
                ILayer layer = null;
                object other = null;
                object index = null;
                axTOCControl2.HitTest(e.x, e.y, ref item, ref map, ref layer, ref other, ref index);
                IMap pMap = axMapControl2.ActiveView.FocusMap;
                if (item == esriTOCControlItem.esriTOCControlItemLayer || layer != null)
                {
                    //预移动图层和鼠标当前位置图层不一致时
                    if (pMoveLayer.Name != layer.Name)
                    {
                        ILayer pTempLayer; 
                        for (int i = 0; i < pMap.LayerCount; i++)
                        {
                            pTempLayer = pMap.get_Layer(i);
                            //获取鼠标当前位置图层的索引值
                            if (pTempLayer.Name == layer.Name)
                            {
                                toIndex = i;
                                break;
                            }
                        }
                        pMap.MoveLayer(pMoveLayer, toIndex);
                        axTOCControl2.ActiveView.Refresh();
                        axTOCControl2.Update();
                    }
                }
            }
        }
        //主题设置按钮
        #region
        private void Btn_Office2013_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.defaultLookAndFeel1.LookAndFeel.SkinName = "Office 2013";

        }

        private void Btn_VS2010_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.defaultLookAndFeel1.LookAndFeel.SkinName = "VS2010";

        }

        private void barButtonItem4_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.defaultLookAndFeel1.LookAndFeel.SkinName = "Office 2010 Blue";

        }

        private void Btn_Office2010Black_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.defaultLookAndFeel1.LookAndFeel.SkinName = "Office 2010 Black";

        }

        private void Btn_DevExpressDarkStyle_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.defaultLookAndFeel1.LookAndFeel.SkinName = "DevExpress  Dark Style";

        }

        private void Btn_SevenClassic_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.defaultLookAndFeel1.LookAndFeel.SkinName = "Seven Classic";

        }
        #endregion

        private void axMapControl1_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            string XYTxt = "经度:" + e.mapX.ToString("#0.0000") + ",纬度:" + e.mapY.ToString("#0.0000") + "    地图比例尺:1:" + axMapControl1.MapScale.ToString("#0");

            Lbl_XYScale.Caption = XYTxt;

        }
        string m_StrLyrName = "";
     
        //查询按钮
        private void Btn_Query_Click(object sender, EventArgs e)
        {            
            //图层名称复选框的Name为 axMapControl1，查询条件文本框的Name为Txt_Query，显示结果GridView为GridView_Info，GridControl的Name为Grid_Info。
            //清空GridView_Info中的现有信息
            GridView_Info.Columns.Clear();                    
             //获取图层名称
             m_StrLyrName = Cbx_Query.Text;
            //获取预查询图层索引号
            int iLyr = 0;
            for (int i = 0; i < axMapControl1.LayerCount; i++)
            {
                if (axMapControl1.get_Layer(i).Name == m_StrLyrName)
                {
                    iLyr = i;
                    break;
                }
            }
            //获取图层信息及要素集
            IFeatureLayer pFeatureLayer = axMapControl1.get_Layer(iLyr) as IFeatureLayer;
            IFeatureClass pFC = pFeatureLayer.FeatureClass;
            //设置查询条件
            IQueryFilter qfilter = new QueryFilterClass();
            //红色‘名称’为预查询图层的预查询字段
            qfilter.WhereClause = "";
            IFeatureSelection pFeatureSelection = pFeatureLayer as IFeatureSelection;
            //清空地图选择集要素，准备向其添加查询结果要素
            axMapControl1.Map.ClearSelection();
            //进行查询
            IFeatureCursor fCursor = pFC.Search(qfilter, false);
            //信息面板预绑定表
            DataTable DTTheme = new DataTable();
            //数据表字段填充
            for (int IntClm = 0; IntClm < fCursor.Fields.FieldCount; IntClm++)
            {
                if (fCursor.Fields.get_Field(IntClm).Name != "Shape")
                    DTTheme.Columns.Add(fCursor.Fields.get_Field(IntClm).Name, typeof(string));
            }
            //准备循环，在选择集中增加查询结果和获取查询结果信息
            for (int j = 0; j < pFC.FeatureCount(qfilter); j++)
            {
                //选择集填充
                try
                {
                    IFeature feature = fCursor.NextFeature();
                    pFeatureSelection.Add(feature);
                    //填充表
                    DataRow dr = DTTheme.NewRow();
                    int IntNodDisplay = 0;
                    for (int IntClm = 0; IntClm < feature.Fields.FieldCount; IntClm++)
                    {
                        if (feature.Fields.get_Field(IntClm).Name != "Shape")
                        {
                            dr[IntClm - IntNodDisplay] = feature.get_Value(IntClm).ToString();
                        }
                        else
                            IntNodDisplay++;
                    }
                    DTTheme.Rows.Add(dr);
                }
                catch (Exception e1)
                {
                    MessageBox.Show(e1.ToString());
                }
                //GridControl绑定结果表
                Grid_Info.DataSource = DTTheme;
                axMapControl1.Refresh();
            }
        }
        //清除选择按钮
        private void Btn_DelSel_Click(object sender, EventArgs e)
        {
            ControlsClearSelectionCommand clearselect = new ControlsClearSelectionCommand();
            clearselect.OnCreate(axMapControl1.Object);
            clearselect.OnClick();

        }
        int m_LayNum = 0;

        //自动读取图层字段
        private void Cbx_Query_SelectedValueChanged(object sender, EventArgs e)
        {

            m_LayNum = Cbx_Query.SelectedIndex;
            this.Cbx_Name.Properties.Items.Clear();
            ILayer Lay = axMapControl1.get_Layer(m_LayNum);
            IFeatureLayer FLay = (IFeatureLayer)Lay;
            IFeatureClass fclass = FLay.FeatureClass;


            try
            {
                for (int i = 0; i < 10; i++)
                {
                    string Name = fclass.GetFeature(i).get_Value(3).ToString();
                    this.Cbx_Name.Properties.Items.AddRange(new object[] { Name });
                }

            }
            catch
            {
            }
            this.Cbx_Name.Text = fclass.GetFeature(0).get_Value(3).ToString();
        }

             
        //空间定位
        private void Grid_Info_DoubleClick(object sender, EventArgs e)
        {
            try
            {

                //获取预查询要素FID
                string IDQuery = GridView_Info.GetFocusedDataRow()["FID"].ToString();

                //循环获取预查询图层ID号
                int iLyr = 0;
                for (int i = 0; i < axMapControl1.LayerCount; i++)
                {
                    if (axMapControl1.get_Layer(i).Name == m_StrLyrName)
                    {
                        iLyr = i;
                        break;
                    }
                }
                //获取预查询图层
                IFeatureLayer pFeatureLayer = axMapControl1.get_Layer(iLyr) as IFeatureLayer;
                IFeatureClass pFC = pFeatureLayer.FeatureClass;

                //进行Search查询
                IQueryFilter qfilter = new QueryFilterClass();
                qfilter.WhereClause = "FID=" + IDQuery + "";

                IFeatureCursor fCursor = pFC.Search(qfilter, false);
                IFeature feature = fCursor.NextFeature();

                //获取查询要素中心点
                IPoint point = new ESRI.ArcGIS.Geometry.Point();
                //如果是面
                if (feature.Shape.GeometryType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon)
                {
                    IArea area = feature.Shape as IArea;
                    point.X = area.Centroid.X;
                    point.Y = area.Centroid.Y;
                }
                //如果是点
                else if (feature.Shape.GeometryType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint)
                {
                    point = feature.Shape as IPoint;
                }
                //如果是线
                else if (feature.Shape.GeometryType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline)
                {
                    IArea area = feature.Extent as IArea;
                    point.X = area.Centroid.X;
                    point.Y = area.Centroid.Y;
                }

                //根据要素中心点进行空间定位
                axMapControl1.CenterAt(point);
                axMapControl1.Update();
                FlashFeature(feature);
            }
            catch { }
       

        }
        //闪烁线
        private void FlashLine(AxMapControl mapControl, IScreenDisplay iScreenDisplay, IGeometry iGeometry)
        {
            ISimpleLineSymbol iLineSymbol;
            ISymbol iSymbol;
            IRgbColor iRgbColor;

            iLineSymbol = new SimpleLineSymbol();
            iLineSymbol.Width = 16;
            iRgbColor = new RgbColor();
            iRgbColor.Blue = 255;
            iRgbColor.Green = 255;
            iLineSymbol.Color = iRgbColor;
            iSymbol = (ISymbol)iLineSymbol;
            iSymbol.ROP2 = esriRasterOpCode.esriROPNotXOrPen;
            mapControl.FlashShape(iGeometry, 3, 500, iSymbol);
        }

        //闪烁面
        private void FlashPolygon(AxMapControl mapControl, IScreenDisplay iScreenDisplay, IGeometry iGeometry)
        {
            ISimpleFillSymbol iFillSymbol;
            ISymbol iSymbol;
            IRgbColor iRgbColor;

            iFillSymbol = new SimpleFillSymbol();
            iFillSymbol.Style = esriSimpleFillStyle.esriSFSSolid;
            iFillSymbol.Outline.Width = 24;

            iRgbColor = new RgbColor();
            iRgbColor.RGB = System.Drawing.Color.FromArgb(100, 180, 180).ToArgb();
            iFillSymbol.Color = iRgbColor;

            iSymbol = (ISymbol)iFillSymbol;
            iSymbol.ROP2 = esriRasterOpCode.esriROPNotXOrPen;
            iScreenDisplay.SetSymbol(iSymbol);
            mapControl.FlashShape(iGeometry, 3, 500, iSymbol);
        }

        //闪烁点
        private void FlashPoint(AxMapControl mapControl, IScreenDisplay iScreenDisplay, IGeometry iGeometry)
        {
            ISimpleMarkerSymbol iMarkerSymbol;
            ISymbol iSymbol;
            IRgbColor iRgbColor;

            iMarkerSymbol = new SimpleMarkerSymbol();
            iMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;
            iRgbColor = new RgbColor();
            iRgbColor.Blue = 255;
            iRgbColor.Green = 255;
            iMarkerSymbol.Color = iRgbColor;

            iSymbol = (ISymbol)iMarkerSymbol;
            iSymbol.ROP2 = esriRasterOpCode.esriROPNotXOrPen;
            mapControl.FlashShape(iGeometry, 3, 500, iSymbol);
        }

        //闪烁目标
        private void FlashFeature(IFeature iFeature)
        {
            IActiveView iActiveView = axMapControl1.Map as IActiveView;
            if (iActiveView != null)
            {
                iActiveView.ScreenDisplay.StartDrawing(0, (short)esriScreenCache.esriNoScreenCache);

                //根据几何类型调用不同的过程
                switch (iFeature.Shape.GeometryType)
                {
                    case esriGeometryType.esriGeometryPolyline:
                        FlashLine(axMapControl1, iActiveView.ScreenDisplay, iFeature.Shape);
                        break;
                    case esriGeometryType.esriGeometryPolygon:
                        FlashPolygon(axMapControl1, iActiveView.ScreenDisplay, iFeature.Shape);
                        break;
                    case esriGeometryType.esriGeometryPoint:
                        FlashPoint(axMapControl1, iActiveView.ScreenDisplay, iFeature.Shape);
                        break;
                    default:
                        break;
                }
                iActiveView.ScreenDisplay.FinishDrawing();
            }
        }
        //寻找要素函数
        private IFeature FindFeature(IFeatureLayer pFeatureLayer, ESRI.ArcGIS.Geometry.IPoint point)
        {
            IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
            ITopologicalOperator pTopo = (ITopologicalOperator)point;

            //ConvertPixelsToMapUnits()为自定义单位转换函数
            IGeometry pBufferGeo = pTopo.Buffer(ConvertPixelsToMapUnits(10) * 10);
            IEnvelope pBufferEnv = pBufferGeo.Envelope;

            ISpatialFilter pSpatiaFilter = new SpatialFilterClass();
            pSpatiaFilter.Geometry = pBufferEnv;
            pSpatiaFilter.GeometryField = pFeatureClass.ShapeFieldName;

            //获取要素种类
            int iType1 = (int)pFeatureClass.ShapeType;
            switch (iType1)
            {
                case 1:
                    pSpatiaFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains; break;
                case 3:
                    pSpatiaFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses; break;
                case 4:
                    pSpatiaFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects; break;
                default: break;
            }

            IFeatureCursor pFeatCursor;
            pFeatCursor = pFeatureClass.Search(pSpatiaFilter, false);
            IFeature pFeat = pFeatCursor.NextFeature();

            return pFeat;
        }
        //寻找最邻近像素
        private double ConvertPixelsToMapUnits(double pixelsUnits)
        {
            IActiveView pActiveView = axMapControl1.ActiveView;
            int pixelsExtent = pActiveView.ScreenDisplay.DisplayTransformation.get_DeviceFrame().right - pActiveView.ScreenDisplay.DisplayTransformation.get_DeviceFrame().left;
            double realWorldDisplayExent = pActiveView.ScreenDisplay.DisplayTransformation.VisibleBounds.Width;
            double SizeOfOnePixes = realWorldDisplayExent / pixelsExtent;
            return SizeOfOnePixes;
        }

        //所有按钮初始化
        private void Btn_Down_Control( DevExpress.XtraBars.BarButtonItem Btn )
        {
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerArrow;
                Btn_Zoomin.Down = false;
                Btn_Zoomout.Down = false;
                Btn_Pan.Down = false;
                Btn_Feature.Down = false;
                Btn_Meature.Down = false;
                Btn_Select.Down = false;
                Btn_TarckPolygon.Down = false;
                Btn.Down = true;
                
         
     
        }
        //添加要素按钮
        private void Btn_Add_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //指定地图鼠标样式
            axMapControl1.CurrentTool = null;


            if (axMapControl1.MousePointer == esriControlsMousePointer.esriPointerCrosshair)
            {
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerArrow;
            }
            else
            {
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            }
            

        }

        private void Btn_Delect_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //指定地图鼠标样式
            axMapControl1.CurrentTool = null;
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerHotLink;


        }

        private void Btn_Edit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //指定地图鼠标样式
            axMapControl1.CurrentTool = null;
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerIdentify;

        }
        //编辑要素保存按钮
        private void Btn_Edit_Save_Click(object sender, EventArgs e)
        {
            int iLyr = 0;
            for (int i = 0; i < axMapControl1.LayerCount; i++)
            {
                if (axMapControl1.get_Layer(i).Name == Info_Dock.Text)
                {
                    iLyr = i;
                    break;
                }
            }
            ITable pTable = axMapControl1.get_Layer(iLyr) as ITable;
            IRow pRow;
            string Sfid = Label_Edit_ID.Text;
            int Ifid = Convert.ToInt32(Sfid);
            //(int)Sfid;
            pRow = pTable.GetRow(Ifid);
            object ve;
            for (int i = 0; i < pRow.Fields.FieldCount; i++)
            {
                if (pRow.Fields.get_Field(i).Name == "名称")
                {
                    ve = Txb_Edit_Name.Text;
                    pRow.set_Value(i, ve);
                }

                if (pRow.Fields.get_Field(i).Name == "类别")
                {
                    ve = Cbx_Edit_Class.Text;
                    pRow.set_Value(i, ve);
                }
                if (pRow.Fields.get_Field(i).Name == "备注")
                {
                    ve = Txb_Edit_Remark.Text;
                    pRow.set_Value(i, ve);
                }
            }
            pRow.Store();

            IActiveView pActiveView = axMapControl1.Map as IActiveView;
            pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, axMapControl1.Map.get_Layer(iLyr), null);
        }  
    }
    }

