using IATK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarChartCreator : MonoBehaviour {

    // input data
    public List<TextAsset> datasets;

    public List<GameObject> barList;


    // Use this for initialization
    void Start()
    {


        barList = new List<GameObject>();
        int count = 0;

        foreach (TextAsset ta in datasets)
        {

            GameObject go = new GameObject("BarCharts-" + count);
            go.transform.parent = this.transform;

            CSVDataSource csvdata = createCSVDataSource(ta.text, go);

            CreateBarchart(csvdata, go, count);
            count++;
            barList.Add(go);
        }
    }

    CSVDataSource createCSVDataSource(string data, GameObject go)
    {
        CSVDataSource dataSource;
        dataSource = go.AddComponent<CSVDataSource>();
        dataSource.load(data, null);
        return dataSource;
    }

    public Vector3 Spherical(float r, float theta, float phi)
    {
        Vector3 pt = new Vector3();
        float snt = (float)Mathf.Sin(theta * Mathf.PI / 180);
        float cnt = (float)Mathf.Cos(theta * Mathf.PI / 180);
        float snp = (float)Mathf.Sin(phi * Mathf.PI / 180);
        float cnp = (float)Mathf.Cos(phi * Mathf.PI / 180);
        pt.x = r * snt * cnp;
        pt.y = r * cnt;
        pt.z = -r * snt * snp;
        return pt;
    }

    // a space time cube
    View CreateBarchart(CSVDataSource csvds, GameObject go, int count)
    {
        // header
        // Date,Time,Lat,Lon,Base
        Gradient g = new Gradient();
        GradientColorKey[] gck = new GradientColorKey[2];
        gck[0] = new GradientColorKey(Color.blue, 0);
        gck[1] = new GradientColorKey(Color.red, 1);
        g.colorKeys = gck;

        // create a view builder with the point topology
        ViewBuilder vb = new ViewBuilder(MeshTopology.Points, "BarCharts-" + count).
            initialiseDataView(csvds.DataCount).
            setDataDimension(csvds["Country"].Data, ViewBuilder.VIEW_DIMENSION.X).
            setDataDimension(csvds["Value"].Data, ViewBuilder.VIEW_DIMENSION.Y).
            setDataDimension(csvds["Year"].Data, ViewBuilder.VIEW_DIMENSION.Z);
        //setSize(csvds["Maximum"].Data).
        //        setSingleColor(Color.white);
        //setColors(csvds["Time"].Data.Select(x => g.Evaluate(x)).ToArray());

        // initialise the view builder wiith thhe number of data points and parent GameOBject

        //Enumerable.Repeat(1f, dataSource[0].Data.Length).ToArray()
        Material mt = IATKUtil.GetMaterialFromTopology(AbstractVisualisation.GeometryType.Bars);
        mt.SetFloat("_MinSize", 0.01f);
        mt.SetFloat("_MaxSize", 0.05f);

        View v = vb.updateView().apply(go, mt);
        Visualisation visualisation = new Visualisation();
        visualisation.dataSource = csvds;

        //  Visualisation

        Vector3 posx = Vector3.zero;
        posx.y = -0.05f;

        DimensionFilter xDimension = new DimensionFilter { Attribute = "Country" };
        //xDimension.minFilter;
        GameObject X_AXIS = CreateAxis(AbstractVisualisation.PropertyType.X, xDimension, posx, new Vector3(0f, 0f, 0f), 0, csvds, visualisation, go);
        Vector3 posy = Vector3.zero;
        posy.x = -0.05f;

        DimensionFilter yDimension = new DimensionFilter { Attribute = "Value" };
        GameObject Y_AXIS = CreateAxis(AbstractVisualisation.PropertyType.Y, yDimension, posy, new Vector3(0f, 0f, 0f), 1, csvds, visualisation, go);
        Vector3 posz = Vector3.zero;
        posz.y = -0.05f;
        posz.x = -0.05f;
        DimensionFilter zDimension = new DimensionFilter { Attribute = "Year" };
        GameObject Z_AXIS = CreateAxis(AbstractVisualisation.PropertyType.Z, zDimension, posz, new Vector3(90f, 0f, 0f), 2, csvds, visualisation, go);

        return v;


    }


    protected GameObject CreateAxis(AbstractVisualisation.PropertyType propertyType, DimensionFilter dimensionFilter, Vector3 position, Vector3 rotation, int index, CSVDataSource csvds, Visualisation vis, GameObject go)
    {
        GameObject AxisHolder;

        AxisHolder = (GameObject)Instantiate(Resources.Load("Axis"));

        AxisHolder.transform.parent = go.transform;
        AxisHolder.name = propertyType.ToString();
        AxisHolder.transform.eulerAngles = (rotation);
        AxisHolder.transform.localPosition = position;

        Axis axis = AxisHolder.GetComponent<Axis>();
        axis.SetDirection((int)propertyType);

        axis.Init(csvds, dimensionFilter, vis);
        BindMinMaxAxisValues(axis, dimensionFilter, vis);

        return AxisHolder;
    }

    protected void BindMinMaxAxisValues(Axis axis, DimensionFilter dim, Visualisation vis)
    {
        object minvalue = vis.dataSource.getOriginalValue(dim.minFilter, dim.Attribute);
        object maxvalue = vis.dataSource.getOriginalValue(dim.maxFilter, dim.Attribute);

        object minScaledvalue = vis.dataSource.getOriginalValue(dim.minScale, dim.Attribute);
        object maxScaledvalue = vis.dataSource.getOriginalValue(dim.maxScale, dim.Attribute);

        axis.AttributeFilter = dim;
        axis.UpdateLabelAttribute(dim.Attribute);

        axis.SetMinNormalizer(dim.minScale);
        axis.SetMaxNormalizer(dim.maxScale);
    }

    //delegate float[] Filter(float[] ar, CSVDataSource csvds, string fiteredValue, string filteringAttribute);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="csvds">CSV data source</param>
    /// <param name="filteringValue"> filtered value</param>
    /// <param name="filteringAttribute"> filtering attribute </param>
    /// <param name="color"></param>
    /// <returns></returns>


    // Update is called once per frame
    void Update()
    {


    }
}
